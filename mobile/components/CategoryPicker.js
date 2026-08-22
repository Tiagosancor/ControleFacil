import { useState } from 'react';
import { Modal, Pressable, ScrollView, Text, View } from 'react-native';
import { useRouter } from 'expo-router';
import { MaterialCommunityIcons } from '@expo/vector-icons';
import { getCategoryIconGlyph, getCategoryIconColor } from '@/constants/categoryIcons';

export function categoryLabel(c) {
  return c.parentCategoryName ? `${c.parentCategoryName} > ${c.name}` : c.name;
}

export function CategoryCircle({ category, size = 36 }) {
  const glyph = getCategoryIconGlyph(category?.iconKey);
  const color = getCategoryIconColor(category?.color);
  return (
    <View
      style={{ height: size, width: size, borderRadius: size / 2, backgroundColor: color }}
      className="items-center justify-center"
    >
      <MaterialCommunityIcons name={glyph} size={size * 0.55} color="#FFFFFF" />
    </View>
  );
}

// Seletor de categoria no padrão "círculo colorido + ícone + nome + rádio" — substitui o
// FormSelect/SelectItem nativo nas telas de Lançamentos, cobrindo categorias de sistema e
// próprias do usuário na mesma lista, com as ações fixas de gerenciamento ao final.
export default function CategoryPicker({ label = 'Categoria', categories, selectedValue, onValueChange, error }) {
  const router = useRouter();
  const [open, setOpen] = useState(false);
  const selected = categories.find(c => String(c.id) === String(selectedValue));

  const select = (category) => {
    onValueChange(String(category.id));
    setOpen(false);
  };

  const goTo = (path) => {
    setOpen(false);
    router.push(path);
  };

  return (
    <View className="mb-4">
      {label && <Text className="text-sm text-text-secondary mb-1">{label}</Text>}

      <Pressable
        onPress={() => setOpen(true)}
        className={`flex-row items-center gap-3 border rounded-md bg-surface px-3 py-2 ${error ? 'border-red-500' : 'border-border'}`}
      >
        {selected ? (
          <>
            <CategoryCircle category={selected} size={28} />
            <Text className="flex-1 text-text-primary" numberOfLines={1}>{categoryLabel(selected)}</Text>
          </>
        ) : (
          <Text className="text-text-secondary">Selecione uma categoria</Text>
        )}
      </Pressable>
      {error && <Text className="text-red-600 text-sm mt-1">{error}</Text>}

      <Modal visible={open} animationType="slide" transparent onRequestClose={() => setOpen(false)}>
        <View className="flex-1 justify-end bg-black/40">
          <Pressable className="absolute inset-0" onPress={() => setOpen(false)} />
          <View className="bg-background rounded-t-2xl max-h-[85%]">
            <View className="flex-row justify-between items-center px-4 pt-4 pb-2">
              <Text className="text-lg font-semibold text-text-primary">Categoria</Text>
              <Pressable onPress={() => setOpen(false)} accessibilityLabel="Fechar">
                <MaterialCommunityIcons name="close" size={24} color="#6B6960" />
              </Pressable>
            </View>

            <ScrollView contentContainerClassName="px-2 pb-2">
              {categories.map(category => (
                <Pressable
                  key={category.id}
                  onPress={() => select(category)}
                  className="flex-row items-center gap-3 px-3 py-2 rounded-md"
                >
                  <CategoryCircle category={category} size={36} />
                  <Text className="flex-1 text-text-primary" numberOfLines={1}>{categoryLabel(category)}</Text>
                  <View
                    className={`h-4 w-4 rounded-full border-2 ${String(category.id) === String(selectedValue) ? 'border-accent bg-accent' : 'border-border'}`}
                  />
                </Pressable>
              ))}
              {!categories.length && (
                <Text className="text-sm text-text-secondary px-3 py-4">Nenhuma categoria cadastrada ainda.</Text>
              )}
            </ScrollView>

            <View className="border-t border-border px-2 py-2">
              <Pressable onPress={() => goTo('/categories/new')} className="px-3 py-3">
                <Text className="text-sm text-accent">Criar categoria</Text>
              </Pressable>
              <Pressable onPress={() => goTo('/categories/new')} className="px-3 py-3">
                <Text className="text-sm text-accent">Criar subcategoria</Text>
              </Pressable>
              <Pressable onPress={() => goTo('/categories')} className="px-3 py-3">
                <Text className="text-sm text-text-secondary">Gerenciar categorias</Text>
              </Pressable>
            </View>
          </View>
        </View>
      </Modal>
    </View>
  );
}
