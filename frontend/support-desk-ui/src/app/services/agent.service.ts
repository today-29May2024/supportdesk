import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Agent } from '../models/agent.model';

@Injectable({ providedIn: 'root' })
export class AgentService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = 'http://localhost:5062/api/agents';

  getAgents(search?: string): Observable<Agent[]> {
    let params = new HttpParams();
    if (search) params = params.set('search', search);

    return this.http.get<Agent[]>(this.apiUrl, { params });
  }

  getAgentById(id: number): Observable<Agent> {
    return this.http.get<Agent>(`${this.apiUrl}/${id}`);
  }
}