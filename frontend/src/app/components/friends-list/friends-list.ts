import { Component, Output, EventEmitter, OnInit, ElementRef, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FriendsService, Friend, UserSearchResult, PendingRequest } from '../../services/friends.service';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { debounceTime, map, startWith } from 'rxjs/operators';
import { Router, RouterModule } from '@angular/router'; 
import { LoadingSpinnerComponent } from '../loading-component/loading-component';

@Component({
  selector: 'app-friends-list',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule, LoadingSpinnerComponent],
  templateUrl: './friends-list.html',
  styleUrl: './friends-list.scss'
})
export class FriendsListComponent implements OnInit {
  friends: Friend[] = [];
  filteredFriends: Friend[] = [];
  pendingRequests: PendingRequest[] = [];
  loading = true;
  error: string = '';

  searchControl = new FormControl('');
  addFriendSearchControl = new FormControl('');

  userSearchResults: UserSearchResult[] = [];
  searchingUsers = false;
  addFriendResults: UserSearchResult[] = [];
  addFriendSearching = false;

  addFriendMode = false;
  addLoadingUserId: number | null = null;
  acceptLoadingUserId: number | null = null;
  rejectLoadingUserId: number | null = null;

  menuOpen = false;
  menuPosition = { top: 0, left: 0 };
  selectedFriend: Friend | null = null;

  @Output() friendClicked = new EventEmitter<Friend>();

  constructor(private friendsService: FriendsService, private router: Router, private elRef: ElementRef) {}

  ngOnInit() {
    this.loadFriends();
    this.loadPendingRequests();

    this.searchControl.valueChanges.pipe(
      startWith(''),
      debounceTime(250),
      map(value => (value ?? '').trim())
    ).subscribe(search => {
      this.filterFriends(search);
    });

    this.addFriendSearchControl.valueChanges.pipe(
      debounceTime(250),
      map(value => (value ?? '').trim())
    ).subscribe(query => {
      if (query.length >= 2) {
        this.addFriendSearching = true;
        this.friendsService.searchUsers(query).subscribe({
          next: (results: UserSearchResult[]) => {
            this.addFriendResults = results;
            this.addFriendSearching = false;
          },
          error: (err) => {
            this.addFriendResults = [];
            this.addFriendSearching = false;
          }
        });
      } else {
        this.addFriendResults = [];
      }
    });
  }

  openFriendMenu(friend: Friend, event: MouseEvent) {
    event.preventDefault();
    event.stopPropagation();
    this.selectedFriend = friend;
    const rect = (event.target as HTMLElement).getBoundingClientRect();
    this.menuPosition = {
      top: rect.bottom + window.scrollY + 2,
      left: rect.right + window.scrollX - 160 
    };
    this.menuOpen = true;
  }

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent) {
    const menuElem = document.querySelector('.friend-popup-menu');
    const btnElems = Array.from(document.querySelectorAll('.friend-menu-btn'));
    const clickedMenu = menuElem && menuElem.contains(event.target as Node);
    const clickedBtn = btnElems.some(btn => btn.contains(event.target as Node));
    if (this.menuOpen && !clickedMenu && !clickedBtn) {
      this.menuOpen = false;
    }

    if (this.addFriendMode) {
      const addOverlay = document.querySelector('.add-friend-overlay');
      if (addOverlay && !addOverlay.contains(event.target as Node)) {
        this.closeAddFriendPanel();
      }
    }
  }

  toggleAddFriendMode() {
    this.addFriendMode = !this.addFriendMode;
    if (!this.addFriendMode) {
      this.addFriendResults = [];
      this.addFriendSearchControl.setValue('');
    }
  }

  getLastOnlineText(dateString?: string): string {
    if (!dateString) return '';
    const date = new Date(dateString);
    const now = new Date();
    const diffMs = now.getTime() - date.getTime();
    const diffMinutes = Math.floor(diffMs / (1000 * 60));
    const diffHours = Math.floor(diffMinutes / 60);
    const diffDays = Math.floor(diffHours / 24);

    if (diffMinutes < 1) {
      return 'just now';
    } else if (diffMinutes < 60) {
      return `${diffMinutes} min ago`;
    } else if (diffHours < 24) {
      return `${diffHours} h ago`;
    } else if (diffDays === 1) {
      return '1 day ago';
    } else {
      return `${diffDays} days ago`;
    }
  }

  loadFriends() {
    this.loading = true;
    this.friendsService.getFriends().subscribe({
      next: (friends: Friend[]) => {
        this.friends = friends;
        this.filterFriends(this.searchControl.value ?? '');
        this.loading = false;
      },
      error: (err) => {
        this.error = 'Could not load friends.';
        this.loading = false;
      }
    });
  }

  loadPendingRequests() {
    this.friendsService.getPendingRequests().subscribe({
      next: (requests: PendingRequest[]) => {
        this.pendingRequests = requests;
      },
      error: (err) => {
        this.pendingRequests = [];
      }
    });
  }

  filterFriends(search: string) {
    if (!search) {
      this.filteredFriends = this.friends;
    } else {
      const lower = search.toLowerCase();
      this.filteredFriends = this.friends.filter(friend =>
        friend.username.toLowerCase().includes(lower) ||
        (friend.fullName?.toLowerCase().includes(lower) ?? false)
      );
    }
  }

  searchUsers(query: string) {
    this.searchingUsers = true;
    this.friendsService.searchUsers(query).subscribe({
      next: (results: UserSearchResult[]) => {
        this.userSearchResults = results;
        this.searchingUsers = false;
      },
      error: (err) => {
        this.userSearchResults = [];
        this.searchingUsers = false;
      }
    });
  }

  addFriendById(userId: number) {
    if (!userId) return;
    this.addLoadingUserId = userId;
    this.friendsService.addFriendById(userId).subscribe({
      next: () => {
        this.loadFriends();
        this.loadPendingRequests();
        this.addFriendResults = [];
        this.addFriendSearchControl.setValue('');
        this.addLoadingUserId = null;
      },
      error: (err) => {
        this.addLoadingUserId = null;
      }
    });
  }

  acceptFriendRequestById(requestId: number, userId: number) {
    this.acceptLoadingUserId = userId;
    this.friendsService.acceptFriendRequest(requestId).subscribe({
      next: () => {
        this.loadFriends();
        this.loadPendingRequests();
        this.acceptLoadingUserId = null;
      },
      error: (err) => {
        this.acceptLoadingUserId = null;
      }
    });
  }

  rejectFriendRequestById(requestId: number, userId: number) {
    this.rejectLoadingUserId = userId;
    this.friendsService.rejectFriendRequest(requestId).subscribe({
      next: () => {
        this.loadFriends();
        this.loadPendingRequests();
        this.rejectLoadingUserId = null;
      },
      error: (err) => {
        this.rejectLoadingUserId = null;
      }
    });
  }

  goToProfile(username: string) {
    if (!username) return;
    this.router.navigate(['/profile', username]);
  }

  trackByFriendId(_: number, friend: Friend) {
    return friend.id;
  }
  trackByUserId(_: number, user: UserSearchResult) {
    return user.id;
  }

  closeAddFriendPanel() {
    this.addFriendMode = false;
    this.addFriendSearchControl.setValue('');
    this.addFriendResults = [];
  }
  onRemoveFriend(friend: Friend) {
    if (!friend || !friend.friendshipId) return;
    this.friendsService.removeFriend(friend.friendshipId).subscribe({
      next: () => {
        this.loadFriends();
        this.menuOpen = false;
      },
      error: (err) => {
        console.error('Error removing friend:', err);
      }
    });
  }
  goToChallenge(friend: Friend) {
    this.menuOpen = false;
    this.router.navigate(['/battle-planner'], {
      queryParams: { opponent: friend.username }
    });
  }
  goToMakGora(friend: Friend) {
    this.menuOpen = false;
    this.router.navigate(['/battle-planner'], {
      queryParams: { opponent: friend.username, makgora: true }
    });
  }
}
