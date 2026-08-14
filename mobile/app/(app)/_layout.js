import { ActivityIndicator, View } from 'react-native';
import { Redirect, Tabs } from 'expo-router';
import { useAuth } from '@/contexts/AuthContext';

export default function AppTabsLayout() {
  const { user, loading } = useAuth();

  if (loading) {
    return (
      <View className="flex-1 items-center justify-center bg-background">
        <ActivityIndicator color="#185FA5" />
      </View>
    );
  }

  if (!user) return <Redirect href="/login" />;

  return (
    <Tabs screenOptions={{ tabBarActiveTintColor: '#185FA5', headerShown: false }}>
      <Tabs.Screen name="transactions" options={{ title: 'Lançamentos' }} />
      <Tabs.Screen name="categories" options={{ title: 'Categorias' }} />
      <Tabs.Screen name="bank-accounts" options={{ title: 'Contas' }} />
    </Tabs>
  );
}
