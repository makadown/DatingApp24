import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { Photo } from '../_models/photo';
import { User } from '../_models/user';
import { environment } from 'environments/environment';

@Injectable({
  providedIn: 'root',
})
export class AdminService {
  baseUrl = environment.apiUrl;

  constructor(private http: HttpClient) {}

  getUsersWithRoles(): Observable<Partial<User[]>> {
    return this.http.get<Partial<User[]>>(
      this.baseUrl + 'admin/users-with-roles'
    );
  }

  updateUserRoles(username: string, roles: string[]): Observable<any> {
    return this.http.post(
      this.baseUrl + 'admin/edit-roles/' + username + '?roles=' + roles,
      {}
    );
  }

  getPhotosForApproval(): Observable<Partial<Photo[]>> {
    return this.http.get<Partial<Photo[]>>(
      this.baseUrl + 'admin/photos-for-approval'
    );
  }

  approvePhoto(photoId: number): Observable<any> {
    return this.http.post(
      this.baseUrl + 'admin/approve-photo/' +  photoId ,
      {}
    );
  }

  rejectPhoto(photoId: number): Observable<any> {
    return this.http.post(
      this.baseUrl + 'admin/reject-photo/' +  photoId ,
      {}
    );
  }
}
