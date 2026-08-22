import {
  Utensils,
  Home,
  ShoppingBag,
  GraduationCap,
  Ticket,
  Landmark,
  MoreHorizontal,
  ArrowLeftRight,
  HeartPulse,
  ClipboardList,
  ShoppingCart,
  Bus,
  Plane,
  Tag,
} from 'lucide-react'

const ICONS = {
  utensils: Utensils,
  home: Home,
  'shopping-bag': ShoppingBag,
  'graduation-cap': GraduationCap,
  ticket: Ticket,
  landmark: Landmark,
  'more-horizontal': MoreHorizontal,
  'arrow-left-right': ArrowLeftRight,
  'heart-pulse': HeartPulse,
  'clipboard-list': ClipboardList,
  'shopping-cart': ShoppingCart,
  bus: Bus,
  plane: Plane,
}

const FALLBACK_ICON = Tag
const FALLBACK_COLOR = '#6B7280'

export function getCategoryIcon(iconKey) {
  return ICONS[iconKey] || FALLBACK_ICON
}

export function getCategoryColor(color) {
  return color || FALLBACK_COLOR
}
