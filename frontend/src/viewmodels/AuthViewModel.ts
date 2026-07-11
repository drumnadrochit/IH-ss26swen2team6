import { loginUser, registerUser } from '../api/auth.api';
import { useAuthStore } from '../store/authStore';
import { getApiErrorMessage } from '../utils/apiError';
import { ViewModel } from './ViewModel';

// Backs both the Login and Register views. Holds the form field state as observable
// properties and exposes login()/register() as commands; the persisted session itself
// lives in the Model layer (authStore) and is only written to on success.
export class AuthViewModel extends ViewModel {
  username = '';
  email = '';
  password = '';
  error = '';
  loading = false;

  setUsername(value: string): void {
    this.username = value;
    this.notify();
  }

  setEmail(value: string): void {
    this.email = value;
    this.notify();
  }

  setPassword(value: string): void {
    this.password = value;
    this.notify();
  }

  async login(): Promise<boolean> {
    this.error = '';
    if (!this.email || !this.password) {
      this.error = 'All fields are required.';
      this.notify();
      return false;
    }
    this.loading = true;
    this.notify();
    try {
      const res = await loginUser({ email: this.email, password: this.password });
      useAuthStore.getState().login(res.accessToken, { userId: res.userId, username: res.username, email: res.email });
      return true;
    } catch (err) {
      this.error = getApiErrorMessage(err, 'Login failed.');
      return false;
    } finally {
      this.loading = false;
      this.notify();
    }
  }

  async register(): Promise<boolean> {
    this.error = '';
    if (!this.username || !this.email || !this.password) {
      this.error = 'All fields are required.';
      this.notify();
      return false;
    }
    if (this.password.length < 8) {
      this.error = 'Password must be at least 8 characters.';
      this.notify();
      return false;
    }
    this.loading = true;
    this.notify();
    try {
      const res = await registerUser({ username: this.username, email: this.email, password: this.password });
      useAuthStore.getState().login(res.accessToken, { userId: res.userId, username: res.username, email: res.email });
      return true;
    } catch (err) {
      this.error = getApiErrorMessage(err, 'Registration failed.');
      return false;
    } finally {
      this.loading = false;
      this.notify();
    }
  }
}
