import { TestBed } from '@angular/core/testing';

import { UserDataService } from './user-data.service';
import { UserService } from '../generated/open-ims-backend';

describe('UserDataService', () => {
  let service: UserDataService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [
      ],
      providers: [
        {
          provide: UserService,
          useValue: {}
        }
      ]
    });
    service = TestBed.inject(UserDataService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
