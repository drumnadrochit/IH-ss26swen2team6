import { useNavigate, Link } from 'react-router-dom';
import { AuthViewModel } from '../../viewmodels/AuthViewModel';
import { useViewModel } from '../../viewmodels/ViewModel';

export default function LoginForm() {
  const vm = useViewModel(() => new AuthViewModel());
  const navigate = useNavigate();

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (await vm.login()) navigate('/');
  };

  return (
    <form onSubmit={handleSubmit} className="flex flex-col gap-3" style={{ maxWidth: 380, margin: '60px auto' }}>
      <div className="card flex flex-col gap-4">
        <h2 className="detail-title">Sign In</h2>
        <div>
          <label className="label">Email</label>
          <input type="email" value={vm.email} onChange={(e) => vm.setEmail(e.target.value)} placeholder="you@example.com" />
        </div>
        <div>
          <label className="label">Password</label>
          <input type="password" value={vm.password} onChange={(e) => vm.setPassword(e.target.value)} placeholder="••••••••" />
        </div>
        {vm.error && <p className="error">{vm.error}</p>}
        <button type="submit" className="btn-primary w-full" disabled={vm.loading}>
          {vm.loading ? 'Signing in...' : 'Sign In'}
        </button>
        <p className="text-sm text-muted text-center">
          No account? <Link to="/register" className="link-accent">Register</Link>
        </p>
      </div>
    </form>
  );
}
