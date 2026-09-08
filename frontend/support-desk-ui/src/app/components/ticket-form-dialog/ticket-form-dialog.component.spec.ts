import { ComponentFixture, TestBed } from '@angular/core/testing';

import { TicketFormDialogComponent } from './ticket-form-dialog.component';

describe('TicketFormDialogComponent', () => {
  let component: TicketFormDialogComponent;
  let fixture: ComponentFixture<TicketFormDialogComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TicketFormDialogComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(TicketFormDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
