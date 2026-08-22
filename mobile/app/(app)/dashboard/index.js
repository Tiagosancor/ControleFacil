import { useCallback, useEffect, useState } from 'react';
import { DeviceEventEmitter, ScrollView, Text, View } from 'react-native';
import { dashboardService } from '@/services/dashboardService';
import Card from '@/components/ui/Card';
import FormSelect, { SelectItem } from '@/components/FormSelect';

const MONTHS = [
  'Janeiro', 'Fevereiro', 'Março', 'Abril', 'Maio', 'Junho',
  'Julho', 'Agosto', 'Setembro', 'Outubro', 'Novembro', 'Dezembro',
];

function formatCurrency(value) {
  return Number(value).toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' });
}

// Mesma leitura "divergente" do gráfico web (receita cresce pra direita/verde,
// despesa pra esquerda/vermelho, a partir de uma linha central) mas com Views +
// flexbox em vez de SVG, para não depender de nenhuma lib de gráfico nativa
// (evita todo o risco de compatibilidade com Expo Go que já tivemos no SDK).
function CategoryBarRow({ item, maxValue }) {
  const isIncome = item.type === 'Income';
  const pct = maxValue > 0 ? Math.min(100, (item.total / maxValue) * 100) : 0;
  return (
    <View className="mb-3">
      <View className="flex-row justify-between mb-1">
        <Text className="text-text-secondary text-xs flex-1 pr-2" numberOfLines={1}>
          {item.categoryGroupName}
        </Text>
        <Text className={`text-xs font-medium ${isIncome ? 'text-income' : 'text-expense'}`}>
          {formatCurrency(item.total)}
        </Text>
      </View>
      <View className="flex-row h-2">
        <View className="flex-1 flex-row justify-end">
          {!isIncome && <View className="h-2 bg-expense rounded-l-full" style={{ width: `${pct}%` }} />}
        </View>
        <View className="w-px bg-border" />
        <View className="flex-1 flex-row justify-start">
          {isIncome && <View className="h-2 bg-income rounded-r-full" style={{ width: `${pct}%` }} />}
        </View>
      </View>
    </View>
  );
}

export default function DashboardScreen() {
  const now = new Date();
  const [year, setYear] = useState(String(now.getFullYear()));
  const [month, setMonth] = useState(String(now.getMonth() + 1));
  const [summary, setSummary] = useState(null);
  const [loading, setLoading] = useState(true);

  const load = useCallback(() => {
    let cancelled = false;
    setLoading(true);
    dashboardService.getMonthlySummary({ year, month })
      .then(res => { if (!cancelled) setSummary(res.data); })
      .catch(() => { if (!cancelled) setSummary(null); })
      .finally(() => { if (!cancelled) setLoading(false); });
    return () => { cancelled = true; };
  }, [year, month]);

  useEffect(() => load(), [load]);

  // Registro rápido (Sprint H) cria fora do fluxo de navegação normal — ver comentário
  // no QuickAddFab.
  useEffect(() => {
    const sub = DeviceEventEmitter.addListener('semeiagrana:transaction-created', load);
    return () => sub.remove();
  }, [load]);

  const breakdown = summary?.categoryBreakdown ?? [];
  const maxValue = breakdown.reduce((max, b) => Math.max(max, b.total), 0);

  return (
    <ScrollView className="flex-1 bg-background" contentContainerClassName="px-4 pt-4 pb-8">
      <Card className="mb-4">
        <View className="flex-row gap-3">
          <View className="flex-1">
            <FormSelect label="Ano" selectedValue={year} onValueChange={setYear}>
              {[now.getFullYear() - 1, now.getFullYear(), now.getFullYear() + 1].map(y => (
                <SelectItem key={y} label={String(y)} value={String(y)} />
              ))}
            </FormSelect>
          </View>
          <View className="flex-1">
            <FormSelect label="Mês" selectedValue={month} onValueChange={setMonth}>
              {MONTHS.map((m, i) => <SelectItem key={m} label={m} value={String(i + 1)} />)}
            </FormSelect>
          </View>
        </View>
      </Card>

      {!loading && summary && (
        <>
          <View className="flex-row gap-3 mb-3">
            <Card className="flex-1">
              <Text className="text-xs text-text-secondary mb-1">Receitas</Text>
              <Text className="text-base font-semibold text-income" numberOfLines={1}>
                {formatCurrency(summary.totalIncome)}
              </Text>
            </Card>
            <Card className="flex-1">
              <Text className="text-xs text-text-secondary mb-1">Despesas</Text>
              <Text className="text-base font-semibold text-expense" numberOfLines={1}>
                {formatCurrency(summary.totalExpense)}
              </Text>
            </Card>
          </View>

          <Card className="mb-4">
            <Text className="text-xs text-text-secondary mb-1">Saldo do mês</Text>
            <Text className={`text-2xl font-semibold ${summary.balance >= 0 ? 'text-income' : 'text-expense'}`}>
              {formatCurrency(summary.balance)}
            </Text>
          </Card>

          <Card>
            <Text className="text-sm text-text-secondary mb-4">Receitas e despesas por grupo</Text>
            {breakdown.length === 0 ? (
              <Text className="text-sm text-text-secondary text-center py-8">Nenhum lançamento neste mês.</Text>
            ) : (
              breakdown.map(item => (
                <CategoryBarRow key={`${item.type}-${item.categoryGroupId}`} item={item} maxValue={maxValue} />
              ))
            )}
          </Card>
        </>
      )}
    </ScrollView>
  );
}
