import { ComponentFixture, TestBed } from '@angular/core/testing';

import { StatusLabelListComponent } from './status-label-list.component';

describe('StatusLabelListComponent', () => {
  let component: StatusLabelListComponent;
  let fixture: ComponentFixture<StatusLabelListComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [StatusLabelListComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(StatusLabelListComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
