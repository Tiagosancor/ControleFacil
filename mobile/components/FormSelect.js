import { Picker } from '@react-native-picker/picker';
import { Text, View } from 'react-native';

export default function FormSelect({ label, selectedValue, onValueChange, error, children, enabled = true }) {
  return (
    <View className="mb-4">
      {label && <Text className="text-sm text-text-secondary mb-1">{label}</Text>}
      <View className={`border border-border rounded-md bg-surface ${enabled ? '' : 'opacity-50'}`}>
        <Picker selectedValue={selectedValue} onValueChange={onValueChange} enabled={enabled}>
          {children}
        </Picker>
      </View>
      {error && <Text className="text-red-600 text-sm mt-1">{error}</Text>}
    </View>
  );
}

export const SelectItem = Picker.Item;
