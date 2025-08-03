// seen-observer.directive.ts
import { Directive, ElementRef, EventEmitter, Output, OnDestroy, AfterViewInit } from '@angular/core';

@Directive({
  selector: '[appSeenObserver]',
  standalone: true
})
export class SeenObserverDirective implements AfterViewInit, OnDestroy {
  @Output() visible = new EventEmitter<void>();
  private observer!: IntersectionObserver;

  constructor(private el: ElementRef) {}

  ngAfterViewInit(): void {
    this.observer = new IntersectionObserver(
      ([entry]) => {
        if (entry.isIntersecting) {
          this.visible.emit();
          this.observer.unobserve(this.el.nativeElement); // one-time trigger
        }
      },
      { threshold: 0.8 }
    );
    this.observer.observe(this.el.nativeElement);
  }

  ngOnDestroy(): void {
    this.observer.disconnect();
  }
}
