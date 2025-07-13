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
  friendshipId: number;
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
  profilePictureUrl?: string;
  fullName: string;
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

  getPendingRequests(): Observable<PendingRequest[]> {
    return this.http.get<PendingRequest[]>(`${this.baseUrl}/friendships/requests`);
  }

  acceptFriendRequest(requestId: number): Observable<any> {
    return this.http.post(`${this.baseUrl}/friendships/accept/${requestId}`, {});
  }

  rejectFriendRequest(requestId: number): Observable<any> {
    return this.http.delete(`${this.baseUrl}/friendships/${requestId}`);
  }
  getAllUsers(): Observable<UserSearchResult[]> {
    return this.http.get<UserSearchResult[]>(`${this.baseUrl}/users`)
  }
  removeFriend(friendshipId: number): Observable<any> {
    return this.http.delete(`${this.baseUrl}/friendships/${friendshipId}`);
  }

}
