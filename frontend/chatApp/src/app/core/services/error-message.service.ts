import { HttpErrorResponse } from '@angular/common/http';
import { Injectable } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class ErrorMessageService {
  getFriendlyMessage(
    error: unknown,
    fallback = 'Something went wrong. Please try again.'
  ): string {
    if (!(error instanceof HttpErrorResponse)) {
      return fallback;
    }

    if (error.status === 0) {
      return 'Network error. Please check your internet connection.';
    }

    switch (error.status) {
      case 400:
        return 'Your request could not be processed. Please check your input and try again.';
      case 401:
        return 'Your session has expired. Please sign in again.';
      case 403:
        return 'You do not have permission to perform this action.';
      case 404:
        return 'The requested resource was not found.';
      case 409:
        return 'This item already exists.';
      case 500:
        return 'Server error. Please try again in a moment.';
      default:
        return fallback;
    }
  }

  isDuplicateUserError(error: unknown): boolean {
    if (!(error instanceof HttpErrorResponse)) {
      return false;
    }

    const serverMessage = this.extractServerMessage(error)?.toLowerCase() ?? '';
    return (
      error.status === 409 ||
      serverMessage.includes('email exists') ||
      serverMessage.includes('user with this email exists') ||
      serverMessage.includes('already exists')
    );
  }

  private extractServerMessage(error: HttpErrorResponse): string | undefined {
    const payload = error.error;
    if (typeof payload === 'string') {
      return payload;
    }

    if (payload && typeof payload === 'object' && 'message' in payload) {
      const msg = (payload as { message?: unknown }).message;
      return typeof msg === 'string' ? msg : undefined;
    }

    return undefined;
  }
}
