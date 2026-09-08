import { Component, EventEmitter, Input, OnInit, Output, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { CreateTicketDto, TICKET_PRIORITY_MAP_BY_STRING, TicketDetailDto, TicketPriority } from '../../models/ticket.model';

@Component({
  selector: 'app-ticket-form-dialog',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './ticket-form-dialog.component.html',
  styleUrls: ['./ticket-form-dialog.component.scss']
})
export class TicketFormDialogComponent implements OnInit {
  private readonly fb = inject(FormBuilder);

  @Input() initialData: TicketDetailDto | null = null;
  @Input() isOpen = false;
  @Input() serverError: string | null = null;
  
  @Output() closeDialog = new EventEmitter<void>();
  @Output() submitForm = new EventEmitter<CreateTicketDto>();

  ticketForm!: FormGroup;
  priorities = Object.values(TicketPriority);
  ticketPriorityMapByString = TICKET_PRIORITY_MAP_BY_STRING;

  ngOnInit(): void {
    this.ticketForm = this.fb.group({
      title: [this.initialData?.title || '', [Validators.required, Validators.maxLength(200)]],
      customerName: [this.initialData?.customerName || '', [Validators.required, Validators.maxLength(100)]],
      customerEmail: [this.initialData?.customerEmail || '', [Validators.required, Validators.email]],
      priority: [this.initialData?.priority || TicketPriority.Normal, [Validators.required]],
      description: [this.initialData?.description || '', [Validators.required, Validators.minLength(10)]]
    });
  }

  onSubmit(): void {
    console.log("Ticket form data:");
    console.log(this.ticketForm.value);
    this.ticketForm.value.priority = this.ticketPriorityMapByString[this.ticketForm.value.priority];
    console.log(this.ticketForm.value.priority);
    if (this.ticketForm.invalid) {
      this.ticketForm.markAllAsTouched();
      return;
    }
    this.submitForm.emit(this.ticketForm.value);
  }

  onCancel(): void {
    this.closeDialog.emit();
  }
}