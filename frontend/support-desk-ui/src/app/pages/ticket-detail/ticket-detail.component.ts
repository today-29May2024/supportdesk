import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { TicketService } from '../../services/ticket.service';
import { AgentService } from '../../services/agent.service';
import { TicketDetailDto, TicketStatus, CreateCommentDto, TICKET_PRIORITY_MAP, TICKET_STATUS_MAP } from '../../models/ticket.model';
import { Agent } from '../../models/agent.model';

@Component({
  selector: 'app-ticket-detail',
  standalone: true,
  imports: [CommonModule, RouterModule, ReactiveFormsModule, FormsModule],
  templateUrl: './ticket-detail.component.html',
  styleUrls: ['./ticket-detail.component.scss']
})
export class TicketDetailComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly ticketService = inject(TicketService);
  private readonly agentService = inject(AgentService);
  private readonly fb = inject(FormBuilder);

  ticket: TicketDetailDto | null = null;
  agents: Agent[] = [];
  
  loading = true;
  actionError: string | null = null;
  selectedAgentId: number | '' = '';

  commentForm!: FormGroup;
  TicketStatus = TicketStatus;

  ticketPriorityMap = TICKET_PRIORITY_MAP;
  ticketStatusMap = TICKET_STATUS_MAP;

  ngOnInit(): void {
    this.commentForm = this.fb.group({
      authorName: ['', [Validators.required]],
      body: ['', [Validators.required, Validators.minLength(3)]]
    });

    const id = Number(this.route.snapshot.paramMap.get('id'));
    if (id) {
      this.loadTicket(id);
      this.loadAgents();
    }
  }

  loadTicket(id: number): void {
    this.loading = true;
    this.ticketService.getTicketById(id).subscribe({
      next: (data) => {
        this.ticket = data;
        this.selectedAgentId = data.assignedAgentId || '';
        this.loading = false;
        if (data.status === TicketStatus.Closed) {
          this.commentForm.disable();
        }
      },
      error: (err) => {
        this.actionError = err.error?.message || 'Failed to load ticket.';
        this.loading = false;
      }
    });
  }

  loadAgents(): void {
    this.agentService.getAgents().subscribe(data => this.agents = data);
  }

  // Requirement #2: Allowed status transitions enforced by UI
  get allowedTransitions(): TicketStatus[] {
    if (!this.ticket || this.ticket.status === TicketStatus.Closed) return [];

    switch (this.ticket.status) {
      case TicketStatus.New:
        return [TicketStatus.InProgress];
      case TicketStatus.InProgress:
        return [TicketStatus.Resolved];
      case TicketStatus.Resolved:
        return [TicketStatus.InProgress, TicketStatus.Closed];
      default:
        return [];
    }
  }

  handleStatusChange(newStatus: TicketStatus): void {
    if (!this.ticket) return;
    this.actionError = null;

    this.ticketService.changeStatus(this.ticket.id, newStatus).subscribe({
      next: (updated) => {
        this.ticket = updated;
        if (updated.status === TicketStatus.Closed) {
          this.commentForm.disable();
        }
      },
      error: (err) => {
        this.actionError = err.error?.message || 'Status transition rejected by server.';
      }
    });
  }

  handleAgentAssign(): void {
    if (!this.ticket || !this.selectedAgentId) return;
    this.actionError = null;

    this.ticketService.assignAgent(this.ticket.id, Number(this.selectedAgentId)).subscribe({
      next: (updated) => this.ticket = updated,
      error: (err) => this.actionError = err.error?.message || 'Failed to assign agent.'
    });
  }

  handleAgentUnassign(): void {
    if (!this.ticket) return;
    this.actionError = null;

    this.ticketService.unassignAgent(this.ticket.id).subscribe({
      next: (updated) => {
        this.ticket = updated;
        this.selectedAgentId = '';
      },
      error: (err) => this.actionError = err.error?.message || 'Failed to unassign agent.'
    });
  }

  handleAddComment(): void {
    if (!this.ticket || this.commentForm.invalid) return;
    this.actionError = null;

    const dto: CreateCommentDto = this.commentForm.value;
    this.ticketService.addComment(this.ticket.id, dto).subscribe({
      next: () => {
        this.commentForm.reset();
        this.loadTicket(this.ticket!.id);
      },
      error: (err) => this.actionError = err.error?.message || 'Failed to post comment.'
    });
  }

  // Requirement #4: Delete ticket with confirmation step
  handleDeleteTicket(): void {
    if (!this.ticket) return;
    if (confirm(`Are you sure you want to delete ticket ${this.ticket.reference}?`)) {
      this.ticketService.deleteTicket(this.ticket.id).subscribe({
        next: () => this.router.navigate(['/tickets'])
      });
    }
  }

  toInt(s: any): number {
    return parseInt(s, 10);
  }
}