import { Pressable, Text } from 'react-native';
import { useAuth } from '@/contexts/AuthContext';

export default function LogoutButton() {
  const { logout } = useAuth();
  return (
    <Pressable onPress={logout} className="mr-4">
      <Text className="text-accent text-sm">Sair</Text>
    </Pressable>
  );
}
