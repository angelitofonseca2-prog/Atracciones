/** @type {import('tailwindcss').Config} */
module.exports = {
  content: ['./app/**/*.{js,jsx,ts,tsx}', './components/**/*.{js,jsx,ts,tsx}'],
  presets: [require('nativewind/preset')],
  theme: {
    extend: {
      colors: {
        primary: '#06b6d4',    // cyan-500
        surface: '#1e293b',    // slate-800
        background: '#0f172a', // slate-900
        card: '#1e293b',
        border: '#334155',     // slate-700
        muted: '#94a3b8',      // slate-400
        danger: '#ef4444',
        success: '#22c55e',
        warning: '#f59e0b',
      },
    },
  },
  plugins: [],
};
