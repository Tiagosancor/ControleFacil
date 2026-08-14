import { Stack } from 'expo-router';
import LogoutButton from '@/components/LogoutButton';

export default function CategoriesLayout() {
  return (
    <Stack screenOptions={{ headerRight: () => <LogoutButton /> }}>
      <Stack.Screen name="index" options={{ title: 'Categorias' }} />
      <Stack.Screen name="new" options={{ title: 'Nova categoria' }} />
      <Stack.Screen name="[id]" options={{ title: 'Editar categoria' }} />
    </Stack>
  );
}
