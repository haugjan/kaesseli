<template>
  <div class="row">
    <q-breadcrumbs class="q-pa-md">
      <q-breadcrumbs-el label="Kontoübersicht" to="/accounts" />
    </q-breadcrumbs>
  </div>
  <div class="row">
    <div v-for="type in accountTypes" :key="type" class="account-type q-pa-md col-md-6 col-sm-12">
      <q-card>
        <q-card-section> <q-avatar size="md" text-color="white" :color="type.color" :icon="type.icon" class="q-mr-sm" />{{ type.name }}</q-card-section>
        <q-card-section>
          <q-table :rows="filteredAccounts(type.name)"
                   :columns="columns"
                   :hide-pagination="true"
                   :rows-per-page-options="[0]"
                   row-key="id">
            <template v-slot:body="props">
              <q-tr :props="props" @click="onRowClick(props.row)">
                <q-td v-for="col in props.cols" :key="col.name" :props="props">
                  {{ col.value }}
                </q-td>

              </q-tr>
            </template>
          </q-table>
        </q-card-section>

      </q-card>
    </div>
  </div>
</template>

<script lang="ts">
  import { defineComponent, ref, Ref, onMounted } from 'vue';
  import axios from 'axios';
  import { useRouter } from 'vue-router'; // Importieren von useRouter


  interface IAccount {
    id: string;
    name: string;
    type: string;
    typeId: number;
    accountBalance: number;
    budget: number;
    budgetBalance: number;
  }

  export default defineComponent({
    name: 'KaesseliAccounts',
    setup() {
      const accounts = ref<IAccount[]>([]);
      const accountTypes = ref([
        { name: 'Einkommen', icon: 'attach_money', color: 'green' },
        { name: 'Ausgaben', icon: 'money_off', color: 'red' },
        { name: 'Aktiv', icon: 'account_balance', color: 'blue' },
        { name: 'Passiv', icon: 'account_balance_wallet', color: 'brown' }
      ]);

      const current: Ref<IAccount | null> = ref(null);
      const router = useRouter();


      const columns = ref([
        { name: 'name', required: true, label: 'Name', align: 'left', field: (row: IAccount) => row.name, sortable: true },
        { name: 'accountBalance', label: 'Kontostand', align: 'right', field: (row: IAccount) => formatNumber(row.accountBalance), sortable: true },
        { name: 'budget', label: 'Budget', align: 'right', field: (row: IAccount) => formatNumber(row.budget), sortable: true },
        { name: 'budgetBalance', label: 'Budgetsaldo', align: 'right', field: (row: IAccount) => formatNumber(row.budgetBalance), sortable: true, classes: 'budgetBalance' },
      ]);

      const fetchAccounts = async () => {
        try {
          const response = await axios.get('https://localhost:7123/accountSummary');
          accounts.value = response.data;
        } catch (error) {
          console.error('There was an error fetching the accounts:', error);
        }
      };

      function formatNumber(value: number, locale: string = navigator.language): string {
        return new Intl.NumberFormat(locale, {
          style: 'decimal',
          minimumFractionDigits: 2,
          maximumFractionDigits: 2,
        }).format(value);
      }

      const filteredAccounts = (type: string) => {
        return accounts.value.filter(account => account.type === type);
      };

      function onRowClick(row: IAccount) {
        router.push(`/accountTable/${row.id}`);
      }

      onMounted(() => {
        fetchAccounts();
      });

      return {
        accounts,
        accountTypes,
        columns,
        filteredAccounts,
        current,
        formatNumber,
        onRowClick
      };
    }
  });
</script>

<style>
  .budgetBalance[data-fldval^='-'] {
    color: red;
    font-weight: bold;
  }

  .budgetBalance:not([data-fldval^='-']) {
    color: green;
    font-weight: bold;
  }
</style>
