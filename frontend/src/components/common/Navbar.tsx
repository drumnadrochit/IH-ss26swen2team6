import { Link, useNavigate } from 'react-router-dom';
import { useAuthStore } from '../../store/authStore';

export default function Navbar() {
  const { user, logout } = useAuthStore();
  const navigate = useNavigate();

  const handleLogout = () => {
    logout();
    navigate('/login');
  };

  return (
    <nav className="navbar flex-row items-center justify-between">
      <Link to="/" className="navbar-brand">
        Tour Planner
      </Link>
      {user && (
        <div className="flex-row items-center gap-4">
          <span className="text-sm">Hello, {user.username}</span>
          <button className="btn-danger btn-sm" onClick={handleLogout}>
            Logout
          </button>
        </div>
      )}
    </nav>
  );
}
