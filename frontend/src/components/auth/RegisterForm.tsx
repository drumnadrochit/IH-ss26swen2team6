import { useNavigate, Link } from 'react-router-dom';
import { AuthViewModel } from '../../viewmodels/AuthViewModel';
import { useViewModel } from '../../viewmodels/ViewModel';

export default function RegisterForm() {
  const vm = useViewModel(() => new AuthViewModel());
  const navigate = useNavigate();

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (await vm.register()) navigate('/');
  };

  return (
    <form onSubmit={handleSubmit} style={{ maxWidth: 380, margin: '60px auto' }}>
      <div className="card flex flex-col gap-4">
        <h2 className="detail-title">Create Account</h2>
        <div>
          <label className="label">Username</label>
          <input value={vm.username} onChange={(e) => vm.setUsername(e.target.value)} placeholder="johndoe" />
        </div>
        <div>
          <label className="label">Email</label>
          <input type="email" value={vm.email} onChange={(e) => vm.setEmail(e.target.value)} placeholder="you@example.com" />
        </div>
        <div>
          <label className="label">Password</label>
          <input type="password" value={vm.password} onChange={(e) => vm.setPassword(e.target.value)} placeholder="min. 8 characters" />
        </div>
        {vm.error && <p className="error">{vm.error}</p>}
        <button type="submit" className="btn-primary w-full" disabled={vm.loading}>
          {vm.loading ? 'Creating account...' : 'Register'}
        </button>
        <p className="text-sm text-muted text-center">
          Already have an account? <Link to="/login" className="link-accent">Sign in</Link>
        </p>
      </div>
    </form>
  );
}
