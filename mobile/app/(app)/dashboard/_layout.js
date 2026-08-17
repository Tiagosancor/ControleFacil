import { Stack } from 'expo-router';
import LogoutButton from '@/components/LogoutButton';

export default function DashboardLayout() {
  return (
    <Stack screenOptions={{ headerRight: () => <LogoutButton /> }}>
      <Stack.Screen name="index" options={{ title: 'Dashboard' }} />
    </Stack>
  );
}
