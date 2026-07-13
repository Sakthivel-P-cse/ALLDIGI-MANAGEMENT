import { ComponentFixture, TestBed } from '@angular/core/testing';

import { StatusLabelFormComponent } from './status-label-form.component';

describe('StatusLabelFormComponent', () => {
  let component: StatusLabelFormComponent;
  let fixture: ComponentFixture<StatusLabelFormComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [StatusLabelFormComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(StatusLabelFormComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
