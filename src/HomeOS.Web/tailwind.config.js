/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    "./src/**/*.{html,ts}",
  ],
  theme: {
    extend: {
      colors: {
        primary: {
          light: '#34d399', // emerald-400
          DEFAULT: '#10b981', // emerald-500
          dark: '#059669', // emerald-600
        },
        surface: {
          DEFAULT: 'rgba(255, 255, 255, 0.7)',
          hover: 'rgba(255, 255, 255, 0.9)'
        },
        background: '#f8fafc' // slate-50
      }
    },
  },
  plugins: [],
}
