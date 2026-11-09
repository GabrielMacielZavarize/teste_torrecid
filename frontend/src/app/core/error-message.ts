import { HttpErrorResponse } from '@angular/common/http';

const unexpectedError = 'Ocorreu um erro inesperado. Tente novamente.';

export function errorMessage(error: unknown): string {
  if (!(error instanceof HttpErrorResponse)) {
    return unexpectedError;
  }

  if (error.status === 0) {
    return 'Não foi possível conectar à API.';
  }

  if (error.status === 403) {
    return 'Você não tem permissão para esta ação.';
  }

  if (error.status >= 500) {
    return unexpectedError;
  }

  // A API responde erros de regra de negócio em "detail" e de validação em "errors".
  if (typeof error.error?.detail === 'string') {
    return error.error.detail;
  }

  return error.status === 400 ? 'Verifique os dados informados.' : unexpectedError;
}
