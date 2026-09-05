import { ChangeDetectorRef, Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { ShoppingListService, ShoppingList, ShoppingListSummary, ShoppingListItem } from '../../core/shopping-list.service';
import { ProductService, Product } from '../../core/product.service';
import { HouseholdService } from '../../core/household.service';

const STATUS_LABEL: Record<string, string> = {
  Pending: 'Pendente',
  InProgress: 'Em Andamento',
  Completed: 'Concluída',
  Cancelled: 'Cancelada'
};

const STATUS_COLOR: Record<string, string> = {
  Pending: 'bg-amber-50 text-amber-700',
  InProgress: 'bg-blue-50 text-blue-700',
  Completed: 'bg-emerald-50 text-emerald-700',
  Cancelled: 'bg-slate-100 text-slate-500'
};

@Component({
  selector: 'app-shopping-lists',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="bg-mesh min-h-screen p-6">
      <nav class="glass-card mb-8 p-4 flex justify-between items-center">
        <div class="flex items-center gap-4">
          <button (click)="router.navigate(['/dashboard'])" class="text-slate-500 hover:text-emerald-600 font-medium text-sm flex items-center gap-1">
            <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M10 19l-7-7m0 0l7-7m-7 7h18"/></svg>
            Voltar
          </button>
          <h1 class="text-xl font-bold text-emerald-600 ml-4 border-l border-slate-200 pl-4">Listas de Compras</h1>
        </div>
        @if (!selectedList) {
          <button (click)="openCreateListModal()" class="btn-primary text-sm px-4 flex items-center gap-2">
            <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4"/></svg>
            Nova Lista
          </button>
        }
      </nav>

      <main class="max-w-6xl mx-auto">

        <!-- Visão geral das listas -->
        @if (!selectedList) {
          @if (isLoading) {
            <div class="flex flex-col items-center justify-center mt-24 gap-4">
              <div class="w-8 h-8 border-4 border-emerald-400 border-t-transparent rounded-full animate-spin"></div>
              <p class="text-slate-500 text-sm">Carregando listas...</p>
            </div>
          }

          @if (!isLoading && lists.length === 0) {
            <div class="text-center py-16 glass-card">
              <div class="text-5xl mb-4">🛒</div>
              <p class="text-slate-600 font-semibold mb-1">Nenhuma lista criada ainda</p>
              <p class="text-slate-400 text-sm mb-6">Crie sua primeira lista de compras para começar.</p>
              <button (click)="openCreateListModal()" class="btn-primary text-sm px-6">Criar primeira lista</button>
            </div>
          }

          @if (!isLoading && lists.length > 0) {
            <div class="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 gap-6">
              @for (list of lists; track list.id) {
                <div class="glass-card p-6 cursor-pointer hover:border-emerald-400 group relative" (click)="selectList(list)">
                  <div class="absolute top-4 right-4 flex gap-2">
                    <span class="px-2 py-1 text-xs rounded-full font-medium {{ statusColor(list.status) }}">
                      {{ statusLabel(list.status) }}
                    </span>
                    <button (click)="deleteList(list.id, $event)" class="text-slate-400 hover:text-red-500 opacity-0 group-hover:opacity-100 transition-opacity p-1 bg-white/80 rounded shadow-sm">
                      <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16"/></svg>
                    </button>
                  </div>
                  <h3 class="text-lg font-bold text-slate-700 mb-1 group-hover:text-emerald-600 pr-24">{{ list.name }}</h3>
                  <p class="text-sm text-slate-500 mb-1">{{ list.totalItems }} itens</p>
                  <p class="text-sm text-slate-400">{{ list.checkedItems }} de {{ list.totalItems }} marcados</p>
                  <!-- Progress bar -->
                  @if (list.totalItems > 0) {
                    <div class="mt-4 h-1.5 bg-slate-100 rounded-full overflow-hidden">
                      <div class="h-full bg-emerald-400 rounded-full transition-all"
                        [style.width]="(list.checkedItems / list.totalItems * 100) + '%'"></div>
                    </div>
                  }
                  <div class="text-sm text-emerald-600 font-medium mt-4">Abrir lista &rarr;</div>
                </div>
              }
            </div>
          }
        }

        <!-- Detalhe da lista selecionada -->
        @if (selectedList) {
          <div class="glass-card p-8">
            <div class="flex justify-between items-start mb-8 pb-6 border-b border-slate-200">
              <div>
                <button (click)="unselectList()" class="text-sm text-slate-500 hover:text-emerald-600 mb-2 flex items-center gap-1">
                  <svg class="w-3 h-3" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M10 19l-7-7m0 0l7-7m-7 7h18"/></svg>
                  Voltar para listas
                </button>
                <h2 class="text-2xl font-bold text-slate-800 flex items-center gap-3">
                  {{ selectedList.name }}
                  <span class="px-2 py-1 text-xs rounded-full font-medium {{ statusColor(selectedList.status) }}">
                    {{ statusLabel(selectedList.status) }}
                  </span>
                </h2>
                <p class="text-sm text-slate-500 mt-1">Criada em {{ selectedList.createdAt | date:'dd/MM/yyyy' }}</p>
              </div>

              <div class="flex gap-3">
                @if (selectedList.status === 'Pending') {
                  <button (click)="startShopping()" class="btn-primary bg-gradient-to-r from-blue-500 to-indigo-500 hover:from-blue-400 hover:to-indigo-400 shadow-blue-500/30 text-sm py-2 px-4">
                    🛒 Iniciar Compras
                  </button>
                }
                @if (selectedList.status === 'InProgress') {
                  <button (click)="completeShopping()" class="btn-primary text-sm py-2 px-4">
                    ✅ Finalizar Compra
                  </button>
                }
              </div>
            </div>

            <div class="grid grid-cols-1 lg:grid-cols-3 gap-8">
              <!-- Adicionar Itens -->
              @if (selectedList.status !== 'Completed' && selectedList.status !== 'Cancelled') {
                <div class="lg:col-span-1">
                  <h3 class="font-semibold text-slate-700 mb-4 flex items-center gap-2">
                    <svg class="w-4 h-4 text-emerald-500" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 6v6m0 0v6m0-6h6m-6 0H6"/></svg>
                    Adicionar Item
                  </h3>

                  <div class="bg-slate-50 rounded-xl p-4 border border-slate-200">
                    @if (products.length === 0) {
                      <p class="text-sm text-slate-500 text-center py-4">
                        Nenhum produto no catálogo.<br>
                        <button (click)="router.navigate(['/products'])" class="text-emerald-600 font-medium hover:underline mt-1 inline-block">Cadastrar produtos →</button>
                      </p>
                    } @else {
                      <label class="block text-sm font-medium text-slate-700 mb-1">Produto</label>
                      <select [(ngModel)]="newItemProductId" class="w-full bg-white border border-slate-200 rounded-lg px-3 py-2 text-sm focus:ring-2 focus:ring-emerald-500/30 outline-none mb-3">
                        <option value="">-- Selecione --</option>
                        @for (p of products; track p.id) {
                          <option [value]="p.id">{{ p.name }}{{ p.brand ? ' (' + p.brand + ')' : '' }}</option>
                        }
                      </select>

                      <div class="grid grid-cols-2 gap-2 mb-4">
                        <div>
                          <label class="block text-sm font-medium text-slate-700 mb-1">Qtd.</label>
                          <input type="number" min="0.01" step="0.01" [(ngModel)]="newItemQuantity"
                            class="w-full bg-white border border-slate-200 rounded-lg px-3 py-2 text-sm focus:ring-2 focus:ring-emerald-500/30 outline-none" />
                        </div>
                        <div>
                          <label class="block text-sm font-medium text-slate-700 mb-1">Unidade</label>
                          <input type="text" [(ngModel)]="newItemUnit" placeholder="kg, L, un..."
                            class="w-full bg-white border border-slate-200 rounded-lg px-3 py-2 text-sm focus:ring-2 focus:ring-emerald-500/30 outline-none" />
                        </div>
                      </div>

                      <button (click)="addItem()" [disabled]="!newItemProductId || isAddingItem" class="w-full btn-primary py-2 text-sm">
                        {{ isAddingItem ? 'Adicionando...' : 'Adicionar à Lista' }}
                      </button>
                    }
                  </div>
                </div>
              }

              <!-- Itens da Lista -->
              <div [class]="(selectedList.status === 'Completed' || selectedList.status === 'Cancelled') ? 'lg:col-span-3' : 'lg:col-span-2'">
                <h3 class="font-semibold text-slate-700 mb-4">
                  Itens ({{ selectedList.items?.length || 0 }})
                  @if (selectedList.items?.length) {
                    <span class="text-sm font-normal text-slate-400 ml-2">
                      {{ checkedCount(selectedList.items) }} de {{ selectedList.items.length }} marcados
                    </span>
                  }
                </h3>

                @if (!selectedList.items?.length) {
                  <div class="text-center py-12 text-slate-400 bg-slate-50 rounded-xl border border-slate-100 border-dashed">
                    <div class="text-3xl mb-2">📋</div>
                    Nenhum item adicionado ainda.
                  </div>
                }

                <div class="flex flex-col gap-2">
                  @for (item of selectedList.items; track item.id) {
                    <div class="flex items-center justify-between p-3 rounded-lg border transition-colors"
                      [class.border-emerald-200]="item.checked"
                      [class.bg-emerald-50]="item.checked"
                      [class.border-slate-200]="!item.checked"
                      [class.bg-white]="!item.checked">

                      <div class="flex items-center gap-3">
                        @if (selectedList.status === 'InProgress') {
                          <button (click)="toggleItemCheck(item)"
                            class="w-6 h-6 rounded-full border-2 flex items-center justify-center transition-all focus:outline-none flex-shrink-0"
                            [class.border-emerald-500]="item.checked"
                            [class.bg-emerald-500]="item.checked"
                            [class.text-white]="item.checked"
                            [class.border-slate-300]="!item.checked">
                            @if (item.checked) {
                              <svg class="w-3.5 h-3.5" fill="none" stroke="currentColor" stroke-width="3" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" d="M5 13l4 4L19 7"/></svg>
                            }
                          </button>
                        }
                        <div>
                          <span [class.line-through]="item.checked" [class.text-slate-400]="item.checked" class="text-slate-700 font-medium text-sm">
                            {{ item.productName }}
                          </span>
                          @if (item.productBrand) {
                            <span class="text-xs text-slate-400 ml-1">{{ item.productBrand }}</span>
                          }
                        </div>
                      </div>

                      <div class="flex items-center gap-3">
                        <span class="text-sm font-bold text-slate-600 bg-slate-100 px-3 py-1 rounded-md">
                          {{ item.quantity }} {{ item.unit || 'un' }}
                        </span>
                        @if (selectedList.status !== 'Completed' && selectedList.status !== 'Cancelled') {
                          <button (click)="removeItem(item.id)" class="text-slate-300 hover:text-red-500 p-1 rounded focus:outline-none transition-colors">
                            <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12"/></svg>
                          </button>
                        }
                      </div>
                    </div>
                  }
                </div>
              </div>
            </div>
          </div>
        }
      </main>

      <!-- Modal Nova Lista -->
      @if (isModalOpen) {
        <div class="fixed inset-0 bg-slate-900/40 backdrop-blur-sm z-50 flex items-center justify-center p-4">
          <div class="glass-card w-full max-w-md p-6 bg-white/95">
            <h2 class="text-xl font-bold text-slate-700 mb-6">Criar Lista de Compras</h2>

            <div class="flex flex-col gap-4">
              <div>
                <label class="block text-sm font-medium text-slate-700 mb-1">Nome da Lista</label>
                <input type="text" [(ngModel)]="newListName" class="input-glass" placeholder="Ex: Compras do Mês (Setembro)" />
              </div>
            </div>

            <div class="flex justify-end gap-3 mt-8">
              <button (click)="closeModal()" class="px-4 py-2 text-sm font-medium text-slate-600 hover:text-slate-800">Cancelar</button>
              <button (click)="createList()" [disabled]="isSaving || !newListName.trim()" class="btn-primary py-2 text-sm">
                {{ isSaving ? 'Criando...' : 'Criar Lista' }}
              </button>
            </div>
          </div>
        </div>
      }
    </div>
  `
})
export class ShoppingListsComponent {
  router = inject(Router);
  private shoppingListService = inject(ShoppingListService);
  private productService = inject(ProductService);
  private householdService = inject(HouseholdService);
  private cdr = inject(ChangeDetectorRef);

  lists: ShoppingListSummary[] = [];
  products: Product[] = [];
  selectedList: ShoppingList | null = null;
  isLoading = true;

  isModalOpen = false;
  isSaving = false;
  newListName = '';

  newItemProductId = '';
  newItemQuantity = 1;
  newItemUnit = '';
  isAddingItem = false;

  ngOnInit() {
    this.loadLists();
    this.loadProducts();
  }

  get activeHouseholdId() {
    return this.householdService.activeHouseholdId;
  }

  checkedCount(items: ShoppingListItem[]): number {
    return items.filter(i => i.checked).length;
  }

  statusLabel(status: string) {
    return STATUS_LABEL[status] ?? status;
  }

  statusColor(status: string) {
    return STATUS_COLOR[status] ?? 'bg-slate-100 text-slate-600';
  }

  loadLists() {
    if (!this.activeHouseholdId) return;
    this.isLoading = true;
    this.shoppingListService.getLists(this.activeHouseholdId).subscribe({
      next: (data) => {
        this.lists = data;
        this.isLoading = false;
        this.cdr.markForCheck();
      },
      error: () => {
        this.isLoading = false;
        this.cdr.markForCheck();
      }
    });
  }

  loadProducts() {
    if (!this.activeHouseholdId) return;
    this.productService.getProducts(this.activeHouseholdId).subscribe({
      next: data => {
        this.products = data;
        this.cdr.markForCheck();
      },
      error: () => {}
    });
  }

  openCreateListModal() {
    this.newListName = '';
    this.isModalOpen = true;
  }

  closeModal() {
    this.isModalOpen = false;
  }

  createList() {
    if (!this.newListName.trim() || !this.activeHouseholdId) return;
    this.isSaving = true;

    this.shoppingListService.createList(this.newListName, this.activeHouseholdId).subscribe({
      next: (list) => {
        this.isSaving = false;
        this.closeModal();
        this.loadLists();
        this.selectListById(list.id);
      },
      error: () => this.isSaving = false
    });
  }

  deleteList(id: string, event: Event) {
    event.stopPropagation();
    if (!confirm('Tem certeza que deseja excluir esta lista?') || !this.activeHouseholdId) return;
    this.shoppingListService.deleteList(id, this.activeHouseholdId).subscribe({
      next: () => this.loadLists()
    });
  }

  selectList(list: ShoppingListSummary) {
    this.selectListById(list.id);
  }

  selectListById(id: string) {
    if (!this.activeHouseholdId) return;
    this.shoppingListService.getListById(id, this.activeHouseholdId).subscribe({
      next: l => {
        this.selectedList = l;
        this.cdr.markForCheck();
      }
    });
  }

  unselectList() {
    this.selectedList = null;
    this.loadLists();
  }

  startShopping() {
    if (!this.selectedList || !this.activeHouseholdId) return;
    this.shoppingListService.startShopping(this.selectedList.id, this.activeHouseholdId).subscribe({
      next: l => {
        this.selectedList = l;
        this.cdr.markForCheck();
      }
    });
  }

  completeShopping() {
    if (!this.selectedList || !this.activeHouseholdId) return;
    if (confirm('Finalizar a compra?')) {
      this.shoppingListService.completeShopping(this.selectedList.id, this.activeHouseholdId).subscribe({
        next: l => {
          this.selectedList = l;
          this.cdr.markForCheck();
        }
      });
    }
  }

  addItem() {
    if (!this.selectedList || !this.newItemProductId || !this.activeHouseholdId) return;
    this.isAddingItem = true;

    this.shoppingListService.addItem(
      this.selectedList.id,
      this.newItemProductId,
      this.newItemQuantity,
      this.activeHouseholdId,
      this.newItemUnit || undefined
    ).subscribe({
      next: () => {
        this.isAddingItem = false;
        this.newItemProductId = '';
        this.newItemQuantity = 1;
        this.newItemUnit = '';
        this.selectListById(this.selectedList!.id);
      },
      error: (err) => {
        this.isAddingItem = false;
        alert(err?.error?.error ?? 'Erro ao adicionar item.');
      }
    });
  }

  removeItem(itemId: string) {
    if (!this.selectedList || !this.activeHouseholdId) return;
    this.shoppingListService.removeItem(this.selectedList.id, itemId, this.activeHouseholdId).subscribe({
      next: () => this.selectListById(this.selectedList!.id)
    });
  }

  toggleItemCheck(item: ShoppingListItem) {
    if (!this.selectedList || !this.activeHouseholdId) return;
    const call = item.checked
      ? this.shoppingListService.uncheckItem(this.selectedList.id, item.id, this.activeHouseholdId)
      : this.shoppingListService.checkItem(this.selectedList.id, item.id, this.activeHouseholdId);

    call.subscribe({
      next: () => this.selectListById(this.selectedList!.id)
    });
  }
}
