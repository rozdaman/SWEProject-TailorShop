import React, { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { ShoppingCart, User, Search, Menu, X } from 'lucide-react';
import { useCart } from '../context/CartContext';

const Navbar = () => {
    const { cart } = useCart();
    const [isMenuOpen, setIsMenuOpen] = useState(false);
    const [searchQuery, setSearchQuery] = useState('');
    const navigate = useNavigate();

    const handleSearch = (e: React.FormEvent) => {
        e.preventDefault();
        if (searchQuery.trim()) {
            navigate(`/?search=${encodeURIComponent(searchQuery)}`);
        }
    };

    const cartItemCount = cart?.items?.reduce((total, item) => total + item.quantity, 0) || 0;

    return (
        <nav className="bg-white shadow-md sticky top-0 z-50">
            <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
                <div className="flex justify-between h-16">

                    {/* Logo */}
                    <div className="flex-shrink-0 flex items-center">
                        <Link to="/" className="text-2xl font-bold text-gray-800 tracking-tight">
                            TAILOR<span className="text-blue-600">.</span>
                        </Link>
                    </div>

                    {/* Desktop Menu */}
                    <div className="hidden md:flex items-center space-x-8">
                        <Link to="/" className="text-gray-600 hover:text-blue-600 font-medium transition">Home</Link>
                        <Link to="/?category=fabric" className="text-gray-600 hover:text-blue-600 font-medium transition">Fabrics</Link>
                        <Link to="/?category=ready-to-wear" className="text-gray-600 hover:text-blue-600 font-medium transition">Ready to Wear</Link>
                    </div>

                    {/* Search, Cart, User */}
                    <div className="flex items-center space-x-4">
                        {/* Search Bar (Desktop) */}
                        <form onSubmit={handleSearch} className="hidden md:flex relative">
                            <input
                                type="text"
                                placeholder="Search products..."
                                className="pl-4 pr-10 py-1 border border-gray-300 rounded-full text-sm focus:outline-none focus:border-blue-500 focus:ring-1 focus:ring-blue-500 transition w-48"
                                value={searchQuery}
                                onChange={(e) => setSearchQuery(e.target.value)}
                            />
                            <button type="submit" className="absolute right-2 top-1.5 text-gray-400 hover:text-blue-600">
                                <Search size={18} />
                            </button>
                        </form>

                        <Link to="/cart" className="relative text-gray-600 hover:text-blue-600 transition">
                            <ShoppingCart size={24} />
                            {cartItemCount > 0 && (
                                <span className="absolute -top-2 -right-2 bg-red-500 text-white text-xs font-bold rounded-full h-5 w-5 flex items-center justify-center">
                                    {cartItemCount}
                                </span>
                            )}
                        </Link>

                        <button className="text-gray-600 hover:text-blue-600 transition">
                            <User size={24} />
                        </button>

                        {/* Mobile menu button */}
                        <div className="md:hidden flex items-center">
                            <button onClick={() => setIsMenuOpen(!isMenuOpen)} className="text-gray-600 hover:text-gray-900 focus:outline-none">
                                {isMenuOpen ? <X size={24} /> : <Menu size={24} />}
                            </button>
                        </div>
                    </div>
                </div>
            </div>

            {/* Mobile Menu */}
            {isMenuOpen && (
                <div className="md:hidden bg-white border-t border-gray-100 pb-4">
                    <div className="px-4 pt-4 pb-2 space-y-2">
                        <form onSubmit={handleSearch} className="flex relative mb-4">
                            <input
                                type="text"
                                placeholder="Search..."
                                className="w-full pl-4 pr-10 py-2 border border-gray-300 rounded-lg focus:outline-none focus:border-blue-500"
                                value={searchQuery}
                                onChange={(e) => setSearchQuery(e.target.value)}
                            />
                            <button type="submit" className="absolute right-3 top-2.5 text-gray-400">
                                <Search size={20} />
                            </button>
                        </form>
                        <Link to="/" className="block py-2 text-gray-600 hover:text-blue-600 font-medium">Home</Link>
                        <Link to="/?category=fabric" className="block py-2 text-gray-600 hover:text-blue-600 font-medium">Fabrics</Link>
                        <Link to="/?category=ready-to-wear" className="block py-2 text-gray-600 hover:text-blue-600 font-medium">Ready to Wear</Link>
                    </div>
                </div>
            )}
        </nav>
    );
};

export default Navbar;
