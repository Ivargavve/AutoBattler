import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { TitleService } from '../../services/title.service';
import { AuthService } from '../../services/auth.service';
import { environment } from '../../../environments/environment';

interface WeeklyLore {
  id: number;
  title: string;
  author: string;
  content: string;
  weeklyMissions: WeeklyMission[];
}

interface WeeklyMission {
  id: string;
  title: string;
  description: string;
  rewardType: string;
  rewardAmount: number;
  rewardItem: string;
}

interface DailyMission {
  id: string;
  title: string;
  description: string;
  rewardType: string;
  rewardAmount: number;
  rewardItem: string;
}

interface TalesResponse {
  currentLore: WeeklyLore;
  dailyMissions: DailyMission[];
  weeklyMissions: WeeklyMission[];
  lastUpdated: string;
  nextDailyReset: string;
  nextWeeklyReset: string;
  missionProgress: { [key: string]: number };
  claimedMissions: { [key: string]: string };
}

interface MissionClaimResponse {
  success: boolean;
  message: string;
  rewardAmount: number;
  rewardType: string;
  rewardItem: string;
}

interface MissionProgressUpdateRequest {
  missionId: string;
  progress: number;
  characterId?: number;
}

@Component({
  selector: 'app-tales-component',
  imports: [CommonModule],
  templateUrl: './tales-component.html',
  styleUrl: './tales-component.scss'
})
export class TalesComponent implements OnInit, OnDestroy {
  talesData: TalesResponse | null = null;
  isLoading = false;
  errorMessage = '';
  claimedMissions = new Set<string>();
  missionProgress = new Map<string, number>();
  
  // Timer properties
  dailyResetTimer: string = '';
  weeklyResetTimer: string = '';
  private timerInterval: any;

  constructor(
    private titleService: TitleService,
    private http: HttpClient,
    private authService: AuthService
  ) {}

  ngOnInit() {
    this.titleService.setTitle('Tales');
    this.loadTalesData();
    this.startTimer();
  }

  ngOnDestroy() {
    this.titleService.setBaseTitle();
    if (this.timerInterval) {
      clearInterval(this.timerInterval);
    }
  }

  loadTalesData() {
    this.isLoading = true;
    this.errorMessage = '';
    
    // Try authenticated endpoint first if user is logged in, fallback to public endpoint
    if (this.authService.isLoggedIn) {
      this.http.get<TalesResponse>(`${environment.apiUrl}/tales/user-missions`).subscribe({
        next: (data) => {
          this.talesData = data;
          this.updateMissionData(data);
          this.isLoading = false;
        },
        error: (error) => {
          console.warn('Authenticated endpoint failed, falling back to public endpoint:', error);
          // Fallback to public endpoint if authenticated request fails
          this.loadPublicTalesData();
        }
      });
    } else {
      this.loadPublicTalesData();
    }
  }

  private loadPublicTalesData() {
    this.http.get<TalesResponse>(`${environment.apiUrl}/tales`).subscribe({
      next: (data) => {
        this.talesData = data;
        this.updateMissionData(data);
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Error loading tales data:', error);
        this.errorMessage = 'Failed to load tales data. Please try again later.';
        this.isLoading = false;
      }
    });
  }

  claimMission(missionId: string, missionType: 'daily' | 'weekly') {
    if (this.claimedMissions.has(missionId)) {
      return;
    }

    if (!this.authService.isLoggedIn) {
      alert('Please log in to claim missions.');
      return;
    }

    this.http.post<MissionClaimResponse>(`${environment.apiUrl}/tales/claim-mission`, {
      missionId: missionId,
      missionType: missionType
    }).subscribe({
      next: (response) => {
        if (response.success) {
          this.claimedMissions.add(missionId);
          // Show success message (you could implement a toast service here)
          alert(`${response.message} You received ${response.rewardAmount} ${response.rewardItem}!`);
          // Reload data to get updated state
          this.loadTalesData();
        } else {
          alert(response.message);
        }
      },
      error: (error) => {
        console.error('Error claiming mission:', error);
        if (error.status === 401) {
          alert('Please log in to claim missions.');
        } else {
          alert('Failed to claim mission. Please try again.');
        }
      }
    });
  }

  getRewardIcon(rewardItem: string): string {
    switch (rewardItem) {
      case 'xp': return '⭐';
      case 'credits': return '💰';
      default: return '🎁';
    }
  }

  getRewardTypeText(rewardType: string): string {
    return rewardType === 'character' ? 'Character' : 'Account';
  }

  updateMissionProgress(missionId: string, progress: number, characterId?: number) {
    this.missionProgress.set(missionId, progress);
    
    if (!this.authService.isLoggedIn) {
      console.log(`Mission ${missionId} progress updated locally (not logged in)`);
      return;
    }
    
    const request: MissionProgressUpdateRequest = {
      missionId: missionId,
      progress: progress,
      characterId: characterId
    };

    this.http.post(`${environment.apiUrl}/tales/update-progress`, request).subscribe({
      next: () => {
        console.log(`Mission ${missionId} progress updated to ${progress}`);
      },
      error: (error) => {
        console.error('Error updating mission progress:', error);
        if (error.status === 401) {
          console.warn('User not authenticated, progress saved locally only');
        }
      }
    });
  }

  formatStoryContent(content: string): string {
    // Convert markdown-style formatting to HTML
    return content
      .replace(/\*\*(.*?)\*\*/g, '<b>$1</b>')  // Bold
      .replace(/\*(.*?)\*/g, '<i>$1</i>')      // Italic
      .replace(/\n\n/g, '</p><p>')             // Paragraph breaks
      .replace(/^/, '<p>')                     // Start paragraph
      .replace(/$/, '</p>');                   // End paragraph
  }

  private updateMissionData(data: TalesResponse) {
    // Update mission progress
    this.missionProgress.clear();
    Object.entries(data.missionProgress).forEach(([key, value]) => {
      this.missionProgress.set(key, value);
    });

    // Update claimed missions
    this.claimedMissions.clear();
    Object.keys(data.claimedMissions).forEach(key => {
      this.claimedMissions.add(key);
    });

    // Update timers
    this.updateTimers(data.nextDailyReset, data.nextWeeklyReset);
  }

  private startTimer() {
    this.timerInterval = setInterval(() => {
      if (this.talesData) {
        this.updateTimers(this.talesData.nextDailyReset, this.talesData.nextWeeklyReset);
      }
    }, 1000); // Update every second
  }

  private updateTimers(dailyReset: string, weeklyReset: string) {
    const now = new Date().getTime();
    const dailyResetTime = new Date(dailyReset).getTime();
    const weeklyResetTime = new Date(weeklyReset).getTime();

    this.dailyResetTimer = this.formatTimeRemaining(dailyResetTime - now);
    this.weeklyResetTimer = this.formatTimeRemaining(weeklyResetTime - now);
  }

  private formatTimeRemaining(milliseconds: number): string {
    if (milliseconds <= 0) {
      return 'Resetting...';
    }

    const days = Math.floor(milliseconds / (1000 * 60 * 60 * 24));
    const hours = Math.floor((milliseconds % (1000 * 60 * 60 * 24)) / (1000 * 60 * 60));
    const minutes = Math.floor((milliseconds % (1000 * 60 * 60)) / (1000 * 60));
    const seconds = Math.floor((milliseconds % (1000 * 60)) / 1000);

    if (days > 0) {
      return `${days}d ${hours}h ${minutes}m`;
    } else if (hours > 0) {
      return `${hours}h ${minutes}m ${seconds}s`;
    } else if (minutes > 0) {
      return `${minutes}m ${seconds}s`;
    } else {
      return `${seconds}s`;
    }
  }

  getMissionProgress(missionId: string): number {
    return this.missionProgress.get(missionId) || 0;
  }

  isMissionCompleted(missionId: string): boolean {
    // For now, missions are completed when they have any progress
    // In a real implementation, you'd check against specific requirements
    return this.getMissionProgress(missionId) > 0;
  }

  isMissionClaimed(missionId: string): boolean {
    // Check if mission is claimed for today (daily) or this week (weekly)
    const now = new Date();
    const todayKey = `${missionId}_${now.getFullYear()}${(now.getMonth() + 1).toString().padStart(2, '0')}${now.getDate().toString().padStart(2, '0')}`;
    const weekNumber = this.getWeekNumber(now);
    const weekKey = `${missionId}_week_${weekNumber}`;
    
    return this.claimedMissions.has(todayKey) || this.claimedMissions.has(weekKey);
  }

  private getWeekNumber(date: Date): number {
    const firstDayOfYear = new Date(date.getFullYear(), 0, 1);
    const pastDaysOfYear = (date.getTime() - firstDayOfYear.getTime()) / 86400000;
    return Math.ceil((pastDaysOfYear + firstDayOfYear.getDay() + 1) / 7);
  }

  completeMission(missionId: string, missionType: 'daily' | 'weekly') {
    if (this.isMissionCompleted(missionId) || this.isMissionClaimed(missionId)) {
      return;
    }

    // Update local progress
    this.missionProgress.set(missionId, 1);
    
    // Send to backend
    this.updateMissionProgress(missionId, 1);
  }
}
