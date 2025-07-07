import { Component, Input, Output, EventEmitter, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FriendsService, Friend, UserSearchResult } from '../../services/friends.service';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { debounceTime, map, startWith } from 'rxjs/operators';
import { Observable } from 'rxjs';

@Component({
  selector: 'app-friends-list',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './friends-list.html',
  styleUrl: './friends-list.scss'
})
export class FriendsListComponent implements OnInit {
  friends: Friend[] = [];
  filteredFriends: Friend[] = [];
  loading = true;
  error: string = '';

  searchControl = new FormControl('');
  userSearchResults: UserSearchResult[] = [];
  searchingUsers = false;
  addLoadingUserId: number | null = null;

  @Output() friendClicked = new EventEmitter<Friend>();

  constructor(private friendsService: FriendsService) {}

  ngOnInit() {
    console.log('FriendsListComponent ngOnInit'); // <-- startar komponenten

    this.loadFriends();

    this.searchControl.valueChanges.pipe(
      startWith(''),
      debounceTime(250),
      map(value => (value ?? '').trim())
    ).subscribe(search => {
      console.log('[searchControl] Changed:', search); // <-- logga varje ändring i fältet
      this.filterFriends(search);
      if (search.length >= 2) {
        console.log('[searchControl] Trigger searchUsers:', search); // <-- logga att vi söker
        this.searchUsers(search);
      } else {
        this.userSearchResults = [];
      }
    });
  }

  loadFriends() {
    this.loading = true;
    console.log('[loadFriends] Called');
    this.friendsService.getFriends().subscribe({
      next: (friends: Friend[]) => {
        console.log('[loadFriends] Loaded friends:', friends);
        this.friends = friends;
        this.filterFriends(this.searchControl.value ?? '');
        this.loading = false;
      },
      error: (err) => {
        console.error('[loadFriends] Error loading friends:', err);
        this.error = 'Could not load friends.';
        this.loading = false;
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
    // Log varje gång friend-listan filtreras
    console.log('[filterFriends] Filtered:', this.filteredFriends);
  }

  searchUsers(query: string) {
    this.searchingUsers = true;
    console.log('[searchUsers] Called with:', query);
    this.friendsService.searchUsers(query).subscribe({
      next: (results: UserSearchResult[]) => {
        console.log('[searchUsers] Results:', results);
        this.userSearchResults = results;
        this.searchingUsers = false;
      },
      error: (err) => {
        console.error('[searchUsers] Error:', err);
        this.userSearchResults = [];
        this.searchingUsers = false;
      }
    });
  }

  addFriendById(userId: number) {
    if (!userId) return;
    this.addLoadingUserId = userId;
    console.log('[addFriendById] Trying to add friend with id:', userId);
    this.friendsService.addFriendById(userId).subscribe({
      next: () => {
        console.log('[addFriendById] Friend added:', userId);
        this.loadFriends();
        this.searchUsers(this.searchControl.value ?? '');
        this.addLoadingUserId = null;
      },
      error: (err) => {
        console.error('[addFriendById] Error:', err);
        this.addLoadingUserId = null;
      }
    });
  }

  trackByFriendId(_: number, friend: Friend) {
    return friend.id;
  }
  trackByUserId(_: number, user: UserSearchResult) {
    return user.id;
  }
  onFriendClick(friend: Friend) {
    this.friendClicked.emit(friend);
  }
}
