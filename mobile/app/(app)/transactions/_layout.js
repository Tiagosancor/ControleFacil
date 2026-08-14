import { Stack } from 'expo-router';
import LogoutButton from '@/components/LogoutButton';

export default function TransactionsLayout() {
  return (
    <Stack screenOptions={{ headerRight: () => <LogoutButton /> }}>
      <Stack.Screen name="index" options={{ title: 'Lançamentos' }} />
      <Stack.Screen name="new" options={{ title: 'Novo lançamento' }} />
      <Stack.Screen name="[id]" options={{ title: 'Editar lançamento' }} />
    </Stack>
  );
}
