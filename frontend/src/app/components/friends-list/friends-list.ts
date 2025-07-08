import { Component, Output, EventEmitter, OnInit } from '@angular/core';
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
  userSearchResults: UserSearchResult[] = [];
  searchingUsers = false;
  addLoadingUserId: number | null = null;
  acceptLoadingUserId: number | null = null;

  @Output() friendClicked = new EventEmitter<Friend>();

  constructor(private friendsService: FriendsService, private router: Router) {}

  ngOnInit() {
    this.loadFriends();
    this.loadPendingRequests();

    this.searchControl.valueChanges.pipe(
      startWith(''),
      debounceTime(250),
      map(value => (value ?? '').trim())
    ).subscribe(search => {
      this.filterFriends(search);
      if (search.length >= 2) {
        this.searchUsers(search);
      } else {
        this.userSearchResults = [];
      }
    });
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
        this.searchUsers(this.searchControl.value ?? '');
        this.addLoadingUserId = null;
      },
      error: (err) => {
        this.addLoadingUserId = null;
      }
    });
  }

  acceptFriendRequest(user: UserSearchResult) {
    let req = this.pendingRequests.find(r => r.requesterId === user.id);

    if (!req) {
      req = this.pendingRequests.find(r =>
        r.requesterUsername?.toLowerCase() === user.username?.toLowerCase()
      );
    }

    if (!req) {
      return;
    }
    this.acceptFriendRequestById(req.id, user.id);
  }

  acceptFriendRequestById(requestId: number, userId: number) {
    this.acceptLoadingUserId = userId;
    this.friendsService.acceptFriendRequest(requestId).subscribe({
      next: () => {
        this.loadFriends();
        this.loadPendingRequests();
        this.searchUsers(this.searchControl.value ?? '');
        this.acceptLoadingUserId = null;
      },
      error: (err) => {
        this.acceptLoadingUserId = null;
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
}
