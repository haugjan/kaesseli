<template>
  <div class="row">
    <q-breadcrumbs class="q-pa-md">
      <q-breadcrumbs-el label="Kontoübersicht" to="/accounts" />
      <q-breadcrumbs-el :label="account?.name" />
    </q-breadcrumbs>
  </div>
  <div class="row q-pa-md" v-if="account">
    <div class="col-12">
      <q-card>

        <q-card-section>
          <span class="header">
            <q-avatar :icon="account.icon" size="md" text-color="white" :color="account.iconColor" />
            {{account.name}} ({{account.type}})
          </span>
        </q-card-section>
        <q-card-section>
          <q-chip color="blue-10" text-color="white" icon="account_balance_wallet">
            {{formatNumber(account.accountBalance)}}
          </q-chip>
          <span v-if="account.typeId === 3 || account.typeId === 4  ">
            <q-chip color="brown-7" text-color="white" icon="savings">
              Σ {{formatNumber(account.budget)}}
            </q-chip>
            <q-chip color="brown-3" text-color="white" icon="savings">
              📅 {{formatNumber(account.currentBudget)}}
            </q-chip>
            <q-chip v-if="account.budgetBalance >= 0" color="green-9" text-color="white" icon="balance">
              {{formatNumber(account.budgetBalance)}}
            </q-chip>
            <q-chip v-if="account.budgetBalance < 0" color="red-14" text-color="white" icon="balance">
              {{ formatNumber(account.budgetBalance)}}
            </q-chip>
          </span>
        </q-card-section>
        <q-card-section>
          <div class="col-md-8 col-sm-12" v-if="account">
            <q-table :rows="account.entries"
                     :columns="columns"
                     :hide-pagination="true"
                     :rows-per-page-options="[0]"
                     row-key="id"
                     wrap-cells="false"
                     dense>
              <template v-slot:body="props">
                <q-tr :class="{'italic-row': props.row.amountType === 1}" :props="props" @click="onRowClick(props.row)">
                  <q-td v-for="col in props.cols" :key="col.name" :props="props" :data-fldval="col.value">
                    {{ col.value }}
                  </q-td>
                </q-tr>
              </template>
            </q-table>
          </div>
        </q-card-section>
      </q-card>



    </div>

  </div>

</template>
<script lang="ts">

  interface IAccount {
    id: string,
    name: string,
    type: string,
    typeId: number,
    accountBalance: number,
    budget: number | null,
    currentBudget: number | null
    budgetBalance: number | null
    entries: IAccountEntry[]
  }

  interface IAccountEntry {
    id: string,
    valueDate: Date,
    description: string,
    amount: number,
    amountType: number,
    otherAccount: string,
    otherAccountId: number
  }



  import { defineComponent, ref, onMounted, inject } from 'vue';
  import { useRoute } from 'vue-router';
  import axios from 'axios';
  import { useQuasar } from 'quasar'; 

  export default defineComponent({
    setup(props) {
      const account = ref<IAccount | null>(null);
      const route = useRoute();
      const $q = useQuasar();

      const columns = ref([
        { name: 'valueDate', required: true, label: 'Datum', align: 'left', field: (row: IAccountEntry) => formatDate(row.valueDate), sortable: true },
        { name: 'description', required: true, label: 'Beschreibung', align: 'left', field: (row: IAccountEntry) => row.description, sortable: true },
        { name: 'otherAccount', required: true, label: 'Gegenkonto', align: 'left', field: (row: IAccountEntry) => row.otherAccount, sortable: true },
        { name: 'Betrag', required: true, label: 'Betrag', align: 'right', field: (row: IAccountEntry) => formatNumber(row.amount), sortable: true, classes: 'budgetBalance' },
      ]);

      const FetchEntries = async () => {
        try {
          const savedPeriodId: string | null = localStorage.getItem('selectedPeriod');
          if (savedPeriodId === null) {
            return;
          }
          const response = await axios.get(`https://localhost:7123/accountingPeriod/${savedPeriodId}/account/${route.params.id}`);
          account.value = response.data;
        } catch (error) {
          $q.notify({
            type: 'negative',
            message: 'There was an error fetching the entries',
            caption: error
          });
        }
      };

      function formatNumber(value: number, locale: string = navigator.language): string {
        return new Intl.NumberFormat(locale, {
          style: 'decimal',
          minimumFractionDigits: 2,
          maximumFractionDigits: 2,
        }).format(value);
      }

      const formatDate = (dateStr: Date) => {
        const date = new Date(dateStr);
        return new Intl.DateTimeFormat(navigator.language, {
          year: 'numeric',
          month: '2-digit',
          day: '2-digit'
        }).format(date);
      };

      onMounted(() => {
        FetchEntries();
      });


      return {
        account,
        FetchEntries,
        formatNumber,
        columns,
        formatDate,
        route
      };

    },
  });
</script>
<style>
  .italic-row {
    font-style: italic;
  }

  .budgetBalance[data-fldval^='-'] {
    color: red;
    font-weight: bold;
  }

  .budgetBalance:not([data-fldval^='-']) {
    color: green;
    font-weight: bold;
  }
</style>  

