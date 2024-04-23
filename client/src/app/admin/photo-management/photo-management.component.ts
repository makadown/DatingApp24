import { Component, OnInit } from '@angular/core';
import { Photo } from '@models/photo';
import { AdminService } from '@services/admin.service';


// tslint:disable: deprecation
@Component({
  selector: 'app-photo-management',
  templateUrl: './photo-management.component.html',
  styleUrls: ['./photo-management.component.css']
})
export class PhotoManagementComponent implements OnInit {
  photos: Photo[] = [];

  constructor(public adminService: AdminService) { }

  ngOnInit(): void {
    this.getPhotosForApproval();
  }

  getPhotosForApproval(): void {
      this.adminService.getPhotosForApproval().subscribe(
        photos => {
          if (photos)
            {
              this.photos = [...(photos as Photo[])];
            }
        }
      );
  }

  approvePhoto(photoId: number): void {
    this.adminService.approvePhoto(photoId).subscribe(
      photos => this.photos = this.photos.filter(p => p.id !== photoId)
    );
  }

  rejectPhoto(photoId: number): void {
    this.adminService.rejectPhoto(photoId).subscribe(
      photos => this.photos = this.photos.filter(p => p.id !== photoId)
    );
  }

}
