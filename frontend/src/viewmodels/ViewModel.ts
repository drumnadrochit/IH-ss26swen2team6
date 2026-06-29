import { useState, useSyncExternalStore } from 'react';

// Base class for all ViewModels in the app's MVVM layer.
//
// View   = React components/pages (presentation only, no business logic)
// ViewModel = subclasses of this class (observable state + commands, e.g. AuthViewModel,
//             TourListViewModel, TourDetailViewModel) - this is what Views bind to
// Model  = the api/*.ts service modules and DTO types - the actual data and server access
//
// A ViewModel exposes plain properties and mutates them through its own methods
// ("commands"). Every mutation calls notify(), which increments `version` and tells
// React (via useSyncExternalStore) to re-render any component bound to this ViewModel.
// This gives the Views property-binding semantics without needing two-way data binding
// machinery: the View reads vm.someProperty and calls vm.someCommand(...).
export abstract class ViewModel {
  version = 0;
  private listeners = new Set<() => void>();

  protected notify(): void {
    this.version++;
    this.listeners.forEach((listener) => listener());
  }

  subscribe = (listener: () => void): (() => void) => {
    this.listeners.add(listener);
    return () => this.listeners.delete(listener);
  };
}

// Binds a React component (View) to a ViewModel instance for its lifetime.
export function useViewModel<T extends ViewModel>(factory: () => T): T {
  const [vm] = useState(factory);
  useSyncExternalStore(vm.subscribe, () => vm.version);
  return vm;
}
