import { HttpErrorResponse } from '@angular/common/http';

interface ProblemDetailsLike {
  detail?: string;
  title?: string;
}

export function extractErrorMessage(
  error: unknown,
  fallback = 'Nao foi possivel concluir a operacao.',
): string {
  if (error instanceof HttpErrorResponse) {
    const problem = error.error as ProblemDetailsLike | string | null;

    if (typeof problem === 'string' && problem.trim().length > 0) {
      return problem;
    }

    if (problem && typeof problem !== 'string' && problem.detail) {
      return problem.detail;
    }

    if (problem && typeof problem !== 'string' && problem.title) {
      return problem.title;
    }
  }

  if (error instanceof Error && error.message.trim().length > 0) {
    return error.message;
  }

  return fallback;
}
