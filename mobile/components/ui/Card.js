import { View } from 'react-native';

export default function Card({ children, className = '' }) {
  return (
    <View className={`bg-surface border border-border rounded-xl p-4 ${className}`}>
      {children}
    </View>
  );
}
