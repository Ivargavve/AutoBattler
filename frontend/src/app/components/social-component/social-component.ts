import { Component, OnInit } from '@angular/core';
import { FriendsService } from '../../services/friends.service';
import { FriendsListComponent } from '../friends-list/friends-list';
import { CommonModule } from '@angular/common';

export interface UserItem {
  id: number;
  username: string;
  fullName: string;
  profilePictureUrl?: string;
  level: number;
}

@Component({
  selector: 'app-social',
  templateUrl: './social-component.html',
  styleUrls: ['./social-component.scss'],
  standalone: true,
  imports: [FriendsListComponent, CommonModule]
})
export class SocialComponent implements OnInit {
  users: UserItem[] = [];
  loading = true;
  error: string | null = null;

  constructor(private friendsService: FriendsService) {}

  ngOnInit() {
    this.friendsService.getAllUsers().subscribe({
      next: (users: any[]) => {
        this.users = users.map(u => ({
          id: u.id,
          username: u.username,
          fullName: u.fullName,
          profilePictureUrl: u.profilePictureUrl,
          level: u.level
        }));
        this.loading = false;
      },
      error: err => {
        this.error = 'Could not load users.';
        this.loading = false;
      }
    });
  }
}
