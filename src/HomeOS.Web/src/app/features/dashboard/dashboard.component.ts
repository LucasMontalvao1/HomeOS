import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { AuthService } from '../../core/auth.service';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="bg-mesh min-h-screen p-6">
      <nav class="glass-card mb-8 p-4 flex justify-between items-center">
        <h1 class="text-xl font-bold text-emerald-600">HomeOS Dashboard</h1>
        <div class="flex items-center gap-4">
          <span class="text-slate-600 font-medium">Olá, {{ user()?.userName }}</span>
          <button (click)="logout()" class="btn-primary text-sm px-4">Sair</button>
        </div>
      </nav>

      <main class="max-w-6xl mx-auto grid grid-cols-1 md:grid-cols-2 gap-6">
        
        <!-- Cards de Módulos (Mock) -->
        <div class="glass-card p-6 cursor-pointer hover:border-emerald-400 group">
          <h2 class="text-xl font-semibold text-slate-700 mb-2 group-hover:text-emerald-600 transition-colors">Listas de Compras</h2>
          <p class="text-slate-500 mb-4">Gerencie as compras da sua casa, extraia notas fiscais com IA.</p>
          <div class="text-sm text-emerald-600 font-medium">3 listas ativas &rarr;</div>
        </div>

        <div class="glass-card p-6 cursor-pointer hover:border-emerald-400 group">
          <h2 class="text-xl font-semibold text-slate-700 mb-2 group-hover:text-emerald-600 transition-colors">Catálogo de Produtos</h2>
          <p class="text-slate-500 mb-4">Veja os produtos recorrentes e histórico de preços.</p>
          <div class="text-sm text-emerald-600 font-medium">120 produtos &rarr;</div>
        </div>

        <div class="glass-card p-6 cursor-pointer hover:border-emerald-400 group md:col-span-2">
          <h2 class="text-xl font-semibold text-slate-700 mb-2 group-hover:text-emerald-600 transition-colors">Gestão da Casa</h2>
          <p class="text-slate-500 mb-4">Convide membros, defina permissões e crie novas casas (Households).</p>
          <div class="text-sm text-emerald-600 font-medium">Ver membros &rarr;</div>
        </div>

      </main>
    </div>
  `
})
export class DashboardComponent {
  private authService = inject(AuthService);
  private router = inject(Router);

  user = this.authService.currentUser;

  logout() {
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}
