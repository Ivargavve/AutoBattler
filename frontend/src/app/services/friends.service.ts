import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface Friend {
  id: number;
  username: string;
  fullName: string;
  profilePictureUrl?: string;
  lastLogin?: string;
  online?: boolean;
}

export interface UserSearchResult {
  id: number;
  username: string;
  fullName: string;
  profilePictureUrl?: string;
  status: 'friend' | 'pending_sent' | 'pending_received' | 'none';
}

export interface PendingRequest {
  id: number;
  requesterId: number;
  requesterUsername: string;
  requesterProfilePictureUrl?: string;
  createdAt: string;
}

@Injectable({ providedIn: 'root' })
export class FriendsService {
  private baseUrl = environment.apiUrl;

  constructor(private http: HttpClient) {}

  getFriends(): Observable<Friend[]> {
    return this.http.get<Friend[]>(`${this.baseUrl}/friendships`);
  }

  addFriendByUsername(username: string): Observable<any> {
    return this.http.post(`${this.baseUrl}/users/add-friend-by-username`, { username });
  }

  searchUsers(query: string): Observable<UserSearchResult[]> {
    return this.http.get<UserSearchResult[]>(`${this.baseUrl}/friendships/search-users?query=${encodeURIComponent(query)}`);
  }

  addFriendById(userId: number): Observable<any> {
    return this.http.post(`${this.baseUrl}/friendships/request/${userId}`, {});
  }
}
