<template>
  <div class="row">
    <q-breadcrumbs class="q-pa-md">
      <q-breadcrumbs-el label="Kontoübersicht" to="/accounts" />
    </q-breadcrumbs>
  </div>
  <div class="row">
    <div v-for="type in accountTypes" :key="type" class="account-type q-pa-md col-md-6 col-sm-12">
      <q-card>
        <span class="header">
          <q-card-section> <q-avatar size="md" text-color="white" :color="type.color" :icon="type.icon" class="q-mr-sm shadow-3" />{{ type.name }}</q-card-section>
        </span>
        <q-card-section>
          <q-table :rows="filteredAccounts(type.name)"
                   :columns="columns"
                   :hide-pagination="true"
                   :rows-per-page-options="[0]"
                   row-key="id"
                   dense>
            <template v-slot:body="props">
              <q-tr :props="props" @click="onRowClick(props.row)">
                <q-td v-for="col in props.cols" :key="col.name" :props="props" :data-fldval="col.value">
                  <span v-if="col.name!=='icon'">{{ col.value }}</span>
                  <span v-if="col.name==='icon'">
                    <q-avatar :icon="props.row.icon" size="sm" text-color="white" :color="props.row.iconColor" :label="props.value" />
                  </span>
                </q-td>

              </q-tr>
            </template>
            <template v-slot:body-cell-icon="props">
              <q-td :props="props" @click="onRowClick(props.row)">
                <div>
                  <q-avatar :icon="props.row.icon" size="sm" text-color="white" :color="props.row.iconColor" :label="props.value" />
                </div>

              </q-td>
            </template>

          </q-table>
        </q-card-section>

      </q-card>
    </div>
  </div>
</template>

<script lang="ts">
  import { defineComponent, ref, Ref, onMounted, inject } from 'vue';
  import axios from 'axios';
  import { useRouter } from 'vue-router'; // Importieren von useRouter


  interface IAccountSummary {
    id: string;
    name: string;
    icon: string;
    iconColor: string;
    type: string;
    typeId: number;
    accountBalance: number;
    budget: number | null;
    currentBudget: number | null;
    budgetBalance: number | null;
  }

  export default defineComponent({
    name: 'KaesseliAccounts',
    setup() {

      const current: Ref<IAccountSummary | null> = ref(null);
      const router = useRouter();
      const selectedPeriod = inject('selectedPeriod');
      const accounts = ref<IAccountSummary[]>([]);

      const accountTypes = ref([
        { name: 'Einkommen', icon: 'attach_money', color: 'green' },
        { name: 'Ausgaben', icon: 'money_off', color: 'red' },
        { name: 'Aktiv', icon: 'account_balance', color: 'blue' },
        { name: 'Passiv', icon: 'account_balance_wallet', color: 'brown' }
      ]);


      const columns = ref([
        { name: 'icon', required: true, label: '', align: 'left', field: (row: IAccountSummary) => row.icon, sortable: true },
        { name: 'name', required: true, label: 'Name', align: 'left', field: (row: IAccountSummary) => row.name, sortable: true },
        { name: 'accountBalance', label: 'Kontostand', align: 'right', field: (row: IAccountSummary) => formatNumber(row.accountBalance), sortable: true },
        { name: 'budget', label: 'Σ Budget', align: 'right', field: (row: IAccountSummary) => formatNumber(row.budget), sortable: true },
        { name: 'currentBudget', label: '📅 Budget', align: 'right', field: (row: IAccountSummary) => formatNumber(row.currentBudget), sortable: true },
        { name: 'budgetBalance', label: 'Budgetsaldo', align: 'right', field: (row: IAccountSummary) => formatNumber(row.budgetBalance), sortable: true, classes: 'budgetBalance' },
      ]);

      const fetchAccounts = async () => {
        try {
          const response = await axios.get(`https://localhost:7123/accountingPeriod/${selectedPeriod.value.id}/accountSummary`);
          accounts.value = response.data;
        } catch (error) {
          console.error('There was an error fetching the accounts:', error);
        }
      };

      function formatNumber(value: number | null, locale: string = navigator.language): string {
        if (!value)
          return "";
        return new Intl.NumberFormat(locale, {
          style: 'decimal',
          minimumFractionDigits: 2,
          maximumFractionDigits: 2,
        }).format(value);
      }

      const filteredAccounts = (type: string) => {
        return accounts.value.filter(account => account.type === type);
      };

      function getColumns(type: string) {
        if (type === 'Aktiv' || type === 'Passiv') {
          return columns.value.filter(column => column.name !== 'budget'
            && column.name !== 'budgetBalance');
        }
        return columns;
      }

      function onRowClick(row: IAccountSummary) {
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
        getColumns,
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
