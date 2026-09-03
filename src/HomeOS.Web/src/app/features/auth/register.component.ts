import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="bg-mesh min-h-screen flex items-center justify-center p-4">
      <div class="glass-card w-full max-w-md p-8">
        
        <div class="text-center mb-8">
          <h1 class="text-3xl font-bold text-emerald-600 mb-2">HomeOS</h1>
          <p class="text-slate-500">Crie sua conta</p>
        </div>

        <div *ngIf="successMessage" class="bg-emerald-50 border border-emerald-200 text-emerald-700 p-4 rounded-xl text-sm mb-4 flex items-start gap-3">
          <svg class="w-5 h-5 mt-0.5 flex-shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z"/></svg>
          <span>{{ successMessage }}</span>
        </div>

        <div *ngIf="errorMessage" class="bg-red-50 border border-red-200 text-red-600 p-3 rounded-xl text-sm mb-4">
          {{ errorMessage }}
        </div>

        <div class="space-y-4">
          <div>
            <label class="block text-sm font-medium mb-1"
              [class.text-red-500]="isNameInvalid"
              [class.text-slate-700]="!isNameInvalid">Nome</label>
            <input 
              type="text"
              [(ngModel)]="name"
              class="input-glass"
              [class.border-red-400]="isNameInvalid"
              [class.bg-red-50]="isNameInvalid"
              placeholder="Seu nome"
              (input)="isNameInvalid = false; errorMessage = ''"
            />
          </div>

          <div>
            <label class="block text-sm font-medium mb-1"
              [class.text-red-500]="isEmailInvalid"
              [class.text-slate-700]="!isEmailInvalid">E-mail</label>
            <input 
              type="text"
              [(ngModel)]="email"
              class="input-glass"
              [class.border-red-400]="isEmailInvalid"
              [class.bg-red-50]="isEmailInvalid"
              placeholder="seu@email.com"
              (input)="isEmailInvalid = false; errorMessage = ''"
            />
          </div>

          <div>
            <label class="block text-sm font-medium mb-1"
              [class.text-red-500]="isPasswordInvalid"
              [class.text-slate-700]="!isPasswordInvalid">Senha</label>
            <input 
              type="password"
              [(ngModel)]="password"
              class="input-glass"
              [class.border-red-400]="isPasswordInvalid"
              [class.bg-red-50]="isPasswordInvalid"
              placeholder="••••••••"
              (input)="isPasswordInvalid = false; errorMessage = ''"
            />
          </div>

          <button 
            type="button"
            (click)="onSubmit()"
            [disabled]="isLoading"
            class="btn-primary w-full flex justify-center items-center h-12 mt-2"
          >
            <span *ngIf="!isLoading">Registrar</span>
            <span *ngIf="isLoading" class="animate-pulse">Criando...</span>
          </button>

          <div class="relative flex items-center py-1">
            <div class="flex-grow border-t border-slate-200"></div>
            <span class="flex-shrink-0 mx-4 text-slate-400 text-sm">ou</span>
            <div class="flex-grow border-t border-slate-200"></div>
          </div>

          <button 
            type="button" 
            disabled
            class="w-full flex justify-center items-center h-12 bg-white text-slate-700 font-medium py-3 px-6 rounded-2xl border border-slate-200 shadow-sm transition-all duration-300 opacity-60 cursor-not-allowed"
          >
            <svg class="w-5 h-5 mr-3" viewBox="0 0 24 24" xmlns="http://www.w3.org/2000/svg">
              <path d="M22.56 12.25c0-.78-.07-1.53-.2-2.25H12v4.26h5.92c-.26 1.37-1.04 2.53-2.21 3.31v2.77h3.57c2.08-1.92 3.28-4.74 3.28-8.09z" fill="#4285F4"/>
              <path d="M12 23c2.97 0 5.46-.98 7.28-2.66l-3.57-2.77c-.98.66-2.23 1.06-3.71 1.06-2.86 0-5.29-1.93-6.16-4.53H2.18v2.84C3.99 20.53 7.7 23 12 23z" fill="#34A853"/>
              <path d="M5.84 14.09c-.22-.66-.35-1.36-.35-2.09s.13-1.43.35-2.09V7.07H2.18C1.43 8.55 1 10.22 1 12s.43 3.45 1.18 4.93l2.85-2.22.81-.62z" fill="#FBBC05"/>
              <path d="M12 5.38c1.62 0 3.06.56 4.21 1.64l3.15-3.15C17.45 2.09 14.97 1 12 1 7.7 1 3.99 3.47 2.18 7.07l3.66 2.84c.87-2.6 3.3-4.53 6.16-4.53z" fill="#EA4335"/>
            </svg>
            Registrar com Google (Em breve)
          </button>
        </div>

        <div class="text-center mt-6">
          <button 
            type="button" 
            (click)="goToLogin()"
            class="text-sm text-emerald-600 font-medium hover:text-emerald-700 hover:underline"
          >
            Já tenho uma conta
          </button>
        </div>

      </div>
    </div>
  `
})
export class RegisterComponent {
  name = '';
  email = '';
  password = '';
  errorMessage = '';
  successMessage = '';
  isLoading = false;
  
  isNameInvalid = false;
  isEmailInvalid = false;
  isPasswordInvalid = false;

  private http = inject(HttpClient);
  private router = inject(Router);

  onSubmit() {
    this.isNameInvalid = !this.name.trim();
    this.isEmailInvalid = !this.email.trim() || !this.email.includes('@');
    this.isPasswordInvalid = !this.password.trim();

    if (this.isNameInvalid || this.isEmailInvalid || this.isPasswordInvalid) {
      if (this.isEmailInvalid && this.email.trim()) {
        this.errorMessage = 'Insira um e-mail válido (com @).';
      } else {
        this.errorMessage = 'Preencha todos os campos destacados.';
      }
      return;
    }
    
    this.isLoading = true;
    this.errorMessage = '';
    
    this.http.post('/api/identity/register', {
      name: this.name.trim(),
      email: this.email.trim(),
      password: this.password
    }).subscribe({
      next: () => {
        this.isLoading = false;
        this.successMessage = 'Conta criada com sucesso! Redirecionando para o login...';
        setTimeout(() => this.router.navigate(['/login']), 2500);
      },
      error: (err) => {
        this.isLoading = false;
        if (err.status === 400 && err.error?.errors) {
          const firstKey = Object.keys(err.error.errors)[0];
          this.errorMessage = err.error.errors[firstKey][0];
        } else {
          this.errorMessage = err.error?.error || 'Erro ao criar conta. Tente novamente.';
        }
      }
    });
  }

  goToLogin() {
    this.router.navigate(['/login']);
  }
}
