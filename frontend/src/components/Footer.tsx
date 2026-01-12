import React from 'react'; // Added import React from 'react'

const Footer = () => {
    return (
        <footer className="bg-gray-900 text-gray-300 py-8 mt-auto">
            <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 text-center md:text-left">
                <div className="grid grid-cols-1 md:grid-cols-3 gap-8">
                    <div>
                        <h3 className="text-xl font-bold text-white mb-4">TAILOR<span className="text-blue-500">.</span></h3>
                        <p className="text-sm text-gray-400">
                            Premium fabrics and ready-to-wear fashion tailored to your needs.
                        </p>
                    </div>
                    <div>
                        <h4 className="font-semibold text-white mb-4">Quick Links</h4>
                        <ul className="space-y-2 text-sm">
                            <li><a href="#" className="hover:text-blue-400 transition">About Us</a></li>
                            <li><a href="#" className="hover:text-blue-400 transition">Contact</a></li>
                            <li><a href="#" className="hover:text-blue-400 transition">Privacy Policy</a></li>
                        </ul>
                    </div>
                    <div>
                        <h4 className="font-semibold text-white mb-4">Contact</h4>
                        <p className="text-sm text-gray-400">Istanbul, Turkey</p>
                        <p className="text-sm text-gray-400">info@tailorproject.com</p>
                    </div>
                </div>
                <div className="border-t border-gray-800 mt-8 pt-6 text-sm text-center">
                    &copy; {new Date().getFullYear()} Tailor Project. All rights reserved.
                </div>
            </div>
        </footer>
    );
};

export default Footer;
