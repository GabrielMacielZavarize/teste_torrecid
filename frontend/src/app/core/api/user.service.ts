import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { Role, User } from '../models';

export interface CreateUserInput {
  username: string;
  password: string;
  role: Role;
}

@Injectable({ providedIn: 'root' })
export class UserService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = '/api/users';

  list(): Observable<User[]> {
    return this.http.get<User[]>(this.baseUrl);
  }

  create(input: CreateUserInput): Observable<User> {
    return this.http.post<User>(this.baseUrl, input);
  }
}
