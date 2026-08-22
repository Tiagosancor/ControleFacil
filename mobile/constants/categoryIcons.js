// IconKey -> glyph do MaterialCommunityIcons (@expo/vector-icons), cobrindo as mesmas
// chaves semânticas do backend (ver docs/reference/sprint-categorias-sistema.md seção 2)
// e o mapa equivalente do web em frontend/src/lib/categoryIcons.js (lucide-react).
export const CATEGORY_ICON_MAP = {
  utensils: 'silverware-fork-knife',
  home: 'home',
  'shopping-bag': 'shopping-outline',
  'graduation-cap': 'school-outline',
  ticket: 'ticket-outline',
  landmark: 'bank-outline',
  'more-horizontal': 'dots-horizontal',
  'arrow-left-right': 'swap-horizontal',
  'heart-pulse': 'heart-pulse',
  'clipboard-list': 'clipboard-list-outline',
  'shopping-cart': 'cart-outline',
  bus: 'bus',
  plane: 'airplane',
};

export const FALLBACK_ICON_GLYPH = 'tag-outline';
export const FALLBACK_ICON_COLOR = '#6B7280';

export function getCategoryIconGlyph(iconKey) {
  return CATEGORY_ICON_MAP[iconKey] || FALLBACK_ICON_GLYPH;
}

export function getCategoryIconColor(color) {
  return color || FALLBACK_ICON_COLOR;
}
