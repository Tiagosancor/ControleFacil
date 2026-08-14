import { Text, TextInput, View } from 'react-native';

export default function FormInput({ label, value, onChangeText, error, ...props }) {
  return (
    <View className="mb-4">
      {label && <Text className="text-sm text-text-secondary mb-1">{label}</Text>}
      <TextInput
        value={value}
        onChangeText={onChangeText}
        placeholderTextColor="#9A988E"
        className="w-full border border-border rounded-md px-3 py-3 text-sm text-text-primary bg-surface"
        {...props}
      />
      {error && <Text className="text-red-600 text-sm mt-1">{error}</Text>}
    </View>
  );
}
