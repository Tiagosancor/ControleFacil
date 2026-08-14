/** @type {import('tailwindcss').Config} */
module.exports = {
  content: ['./src/pages/**/*.{js,ts,jsx,tsx}', './src/components/**/*.{js,ts,jsx,tsx}'],
  theme: {
    extend: {
      colors: {
        background: '#FAFAF8',
        surface: '#FFFFFF',
        border: '#E5E3DC',
        'text-primary': '#1F1E1B',
        'text-secondary': '#6B6960',
        'text-muted': '#9A988E',
        accent: '#185FA5',
        'accent-hover': '#0C447C',
        income: '#1E7A46',
        expense: '#B3261E',
      },
    },
  },
  plugins: [],
}
