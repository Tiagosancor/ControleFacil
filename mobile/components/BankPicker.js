import { useEffect, useRef, useState } from 'react';
import { ActivityIndicator, Image, Modal, Pressable, ScrollView, Text, TextInput, View } from 'react-native';
import { MaterialCommunityIcons } from '@expo/vector-icons';
import { bankService } from '@/services/bankService';

function BankLogo({ bank, size = 32 }) {
  const [failed, setFailed] = useState(false);

  if (!bank.logoUrl || failed) {
    return (
      <View
        style={{ height: size, width: size, borderRadius: size / 2 }}
        className="bg-accent items-center justify-center"
      >
        <Text className="text-white text-xs font-semibold">{bank.name.trim().charAt(0).toUpperCase()}</Text>
      </View>
    );
  }

  return (
    <Image
      source={{ uri: bank.logoUrl }}
      style={{ height: size, width: size, borderRadius: size / 2 }}
      className="bg-surface border border-border"
      resizeMode="contain"
      onError={() => setFailed(true)}
    />
  );
}

// Busca de banco com logo — modal com campo de busca (debounce), igual ao padrão do
// CategoryPicker mas com <Image>+fallback no lugar do círculo de ícone. Sempre opcional:
// Nome (apelido da conta) continua livre, independente do banco escolhido aqui.
export default function BankPicker({ label = 'Banco (opcional)', value, onChange }) {
  const [open, setOpen] = useState(false);
  const [query, setQuery] = useState('');
  const [results, setResults] = useState([]);
  const [loading, setLoading] = useState(false);
  const debounceRef = useRef(null);

  useEffect(() => {
    if (!open) return undefined;
    if (debounceRef.current) clearTimeout(debounceRef.current);

    debounceRef.current = setTimeout(() => {
      setLoading(true);
      bankService.search({ search: query || undefined })
        .then(res => setResults(res.data))
        .catch(() => setResults([]))
        .finally(() => setLoading(false));
    }, 300);

    return () => clearTimeout(debounceRef.current);
  }, [query, open]);

  const openModal = () => {
    setQuery('');
    setOpen(true);
  };

  const select = (bank) => {
    onChange(bank);
    setOpen(false);
  };

  const clear = () => {
    onChange(null);
    setOpen(false);
  };

  return (
    <View className="mb-4">
      {label && <Text className="text-sm text-text-secondary mb-1">{label}</Text>}

      <Pressable
        onPress={openModal}
        className="flex-row items-center gap-3 border border-border rounded-md bg-surface px-3 py-2"
      >
        {value ? (
          <>
            <BankLogo bank={value} size={26} />
            <Text className="flex-1 text-text-primary" numberOfLines={1}>{value.name}</Text>
          </>
        ) : (
          <Text className="text-text-secondary">Selecionar banco</Text>
        )}
      </Pressable>

      <Modal visible={open} animationType="slide" transparent onRequestClose={() => setOpen(false)}>
        <View className="flex-1 justify-end bg-black/40">
          <Pressable className="absolute inset-0" onPress={() => setOpen(false)} />
          <View className="bg-background rounded-t-2xl max-h-[85%]">
            <View className="flex-row justify-between items-center px-4 pt-4 pb-2">
              <Text className="text-lg font-semibold text-text-primary">Banco</Text>
              <Pressable onPress={() => setOpen(false)} accessibilityLabel="Fechar">
                <MaterialCommunityIcons name="close" size={24} color="#6B6960" />
              </Pressable>
            </View>

            <View className="px-4 pb-2">
              <TextInput
                value={query}
                onChangeText={setQuery}
                placeholder="Buscar banco pelo nome..."
                placeholderTextColor="#9A988E"
                autoCorrect={false}
                className="w-full border border-border rounded-md px-3 py-3 text-sm text-text-primary bg-surface"
              />
            </View>

            <ScrollView contentContainerClassName="px-2 pb-2">
              {loading ? (
                <ActivityIndicator className="my-4" color="#185FA5" />
              ) : (
                results.map(bank => (
                  <Pressable
                    key={bank.ispb}
                    onPress={() => select(bank)}
                    className="flex-row items-center gap-3 px-3 py-2 rounded-md"
                  >
                    <BankLogo bank={bank} />
                    <Text className="flex-1 text-text-primary" numberOfLines={1}>{bank.name}</Text>
                  </Pressable>
                ))
              )}
              {!loading && !results.length && (
                <Text className="text-sm text-text-secondary px-3 py-4">Nenhum banco encontrado.</Text>
              )}
            </ScrollView>

            {value && (
              <View className="border-t border-border px-2 py-2">
                <Pressable onPress={clear} className="px-3 py-3">
                  <Text className="text-sm text-red-600">Remover banco selecionado</Text>
                </Pressable>
              </View>
            )}
          </View>
        </View>
      </Modal>
    </View>
  );
}
