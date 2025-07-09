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
  addFriendSearchControl = new FormControl('');

  userSearchResults: UserSearchResult[] = [];
  searchingUsers = false;
  addFriendResults: UserSearchResult[] = [];
  addFriendSearching = false;

  addFriendMode = false;
  addLoadingUserId: number | null = null;
  acceptLoadingUserId: number | null = null;
  rejectLoadingUserId: number | null = null;

  @Output() friendClicked = new EventEmitter<Friend>();

  constructor(private friendsService: FriendsService, private router: Router) {}

  ngOnInit() {
    this.loadFriends();
    this.loadPendingRequests();

    // Vänlistans sökruta (filtrerar bara egna vänner)
    this.searchControl.valueChanges.pipe(
      startWith(''),
      debounceTime(250),
      map(value => (value ?? '').trim())
    ).subscribe(search => {
      this.filterFriends(search);
    });

    // Sökfältet i modal för att lägga till nya vänner
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

  toggleAddFriendMode() {
    this.addFriendMode = !this.addFriendMode;
    if (!this.addFriendMode) {
      this.addFriendResults = [];
      this.addFriendSearchControl.setValue('');
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

  // OBS! userSearchResults används bara om du har gamla sök på alla – annars kan du ta bort searchUsers helt.
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
        // Nollställ även popup-resultat direkt efter add, för tydlig feedback
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
}
