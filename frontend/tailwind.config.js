/** @type {import('tailwindcss').Config} */
module.exports = {
  content: ['./src/pages/**/*.{js,ts,jsx,tsx}', './src/components/**/*.{js,ts,jsx,tsx}'],
  theme: {
    extend: {
      colors: {
        background: '#F5F6F1',
        surface: '#FFFFFF',
        panel: '#EFF2ED',
        border: '#CBD5CE',
        'border-strong': '#B9C6BC',
        'text-primary': '#1E2A24',
        'text-secondary': '#4B5A52',
        'text-muted': '#5E6D64',
        primary: '#285649',
        'primary-hover': '#1D4137',
        'primary-soft': '#E7EEEA',
        link: '#1E2A24',
        gold: '#8A6A1B',
        'gold-soft': '#DAB946',
        'gold-wash': '#FBF3DC',
        terracotta: '#A6503B',
        'terracotta-wash': '#F6E9E5',
        income: '#8A6A1B',
        expense: '#A6503B',
      },
      fontFamily: {
        heading: ['Fraunces', 'ui-serif', 'Georgia', 'serif'],
        sans: ['Manrope', 'ui-sans-serif', 'system-ui', 'sans-serif'],
      },
      boxShadow: {
        soft: '0 1px 2px rgba(30, 42, 36, 0.06), 0 1px 1px rgba(30, 42, 36, 0.04)',
      },
      borderRadius: {
        md: '7px',
        xl: '10px',
      },
    },
  },
  plugins: [],
}
