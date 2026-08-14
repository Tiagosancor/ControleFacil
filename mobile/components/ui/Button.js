import { Pressable, Text } from 'react-native';

const VARIANTS = {
  primary: 'bg-accent',
  secondary: 'bg-transparent border border-border',
  danger: 'bg-red-600',
};

const TEXT_VARIANTS = {
  primary: 'text-white',
  secondary: 'text-text-primary',
  danger: 'text-white',
};

export default function Button({ variant = 'secondary', onPress, disabled, children, className = '' }) {
  return (
    <Pressable
      onPress={onPress}
      disabled={disabled}
      className={`rounded-md px-4 py-3 items-center ${VARIANTS[variant]} ${disabled ? 'opacity-50' : ''} ${className}`}
    >
      <Text className={`text-sm font-medium ${TEXT_VARIANTS[variant]}`}>{children}</Text>
    </Pressable>
  );
}
