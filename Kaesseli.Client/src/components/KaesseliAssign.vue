<template>
  <div v-if="transaction" class="row q-pa-md">
    <q-card flat bordered class="my-card">
      <q-card-section class="col-md-6 col-sm-12">
        <div class="text-h6">
          {{formatDate( transaction.valueDate)}}
          <p>
            <q-chip v-if="transaction.amount >= 0" color="green-9" text-color="white" icon="attach_money">
              {{formatNumber(transaction.amount)}}
            </q-chip>
            <q-chip v-if="transaction.amount < 0" color="red-14" text-color="white" icon="money_off">
              {{ formatNumber(transaction.amount)}}
            </q-chip>
          </p>
        </div>
      </q-card-section>

      <q-card-section class="q-pt-none">
        {{transaction.accountName}}
      </q-card-section>

      <q-separator inset />

      <q-card-section class="q-pt-none">
        {{transaction.description}}
      </q-card-section>
    </q-card>
  </div>
  <div v-if="transaction" class="row q-pa-md">
    <q-input ref="filterInput" filled dense v-model="filterText" label="Filter" @keyup.enter="handleEnter" />
  </div>

  <div class="q-pa-md">
    <q-list dense class="row qp-pa-md bg-blue-3">
      <q-item class="col-lg-4 col-md-6 col-sm-12" dense v-for="account in splittingAccounts" v-bind:key="account.accountId">
        <q-item-section dense>
          <q-item-label>
            <q-chip size="md" :color="account.suggestedAccount.accountIconColor" :icon="account.suggestedAccount.accountIcon" text-color="white" square>{{account.suggestedAccount.accountName}}</q-chip>
          </q-item-label>
        </q-item-section>
        <q-item-section dense>
          <q-input v-model="account.amount" type="number" label="Betrag" />
        </q-item-section>

      </q-item>
    </q-list>
    <q-btn v-if="splittingAccounts.length > 0" class="row text-primary" @click="splitAmount">Betrag aufteilen</q-btn>
  </div>

  <div v-if="transaction" class="row q-pa-md">
    <div v-for="account in filteredAccounts" :key="account.id">
      <q-chip clickable @click="onClick(account,  $event)" size="md" :color="account.accountIconColor" :icon="account.accountIcon" text-color="white" square> {{account.accountName}}</q-chip>

    </div>
  </div>
</template>

<script lang="ts">

  interface ISuggestedAccount {
    accountId: number;
    accountName: string;
    accountType: string;
    accountTypeId: number;
    accountIcon: string;
    accountIconColor: string;
    relevance: number;
  }

  interface ISplitAccount {
    suggestedAccount: ISuggestedAccount;
    amount: number;
  }

  interface ITransaction {
    id: string;
    amount: number;
    description: string;
    valueDate: Date;
    accountName: string;
    accountType: string;
    accountTypeId: number;
    suggestedAccounts: ISuggestedAccount[];
  }

  import { defineComponent, ref, onMounted, computed, nextTick } from 'vue';
  import axios from 'axios';
  import { useQuasar } from 'quasar'
import { forEachChild } from 'typescript';

  export default defineComponent({
    setup() {

      const transaction = ref<ITransaction | null>(null);
      const filterText = ref<string>("");
      const filterInput = ref(null);
      const splittingAccounts = ref<ISplitAccount[]>([]);
      const $q = useQuasar();

const filteredAccounts = computed(() => {
  if (!transaction.value) {
    return [];
  }

  const splittingAccountIds = splittingAccounts.value.map(a => a.suggestedAccount.accountId);

  return transaction.value.suggestedAccounts.filter((account) => {
    return account.accountName.toLowerCase().includes(filterText.value.toLowerCase())
           && !splittingAccountIds.includes(account.accountId);
  });
});

      const FetchTransaction = async () => {
        try {
          const response = await axios.get('https://localhost:7123/transaction/nextOpen');
          transaction.value = response.data;
        } catch (error) {
          $q.notify({
            type: 'negative',
            message: 'Error updating the transaction: ' + error
          });
        }
      };


      const formatDate = (dateStr: Date) => {
        const date = new Date(dateStr);
        return new Intl.DateTimeFormat('de-CH', { // 'de-CH' für Schweizerdeutsch, um das gewünschte Format zu erhalten
          weekday: 'long', // volle Bezeichnung des Wochentags
          year: 'numeric',
          month: 'long', // volle Bezeichnung des Monats
          day: '2-digit'
        }).format(date);
      };

      function formatNumber(value: number, locale: string = navigator.language): string {
        return new Intl.NumberFormat(locale, {
          style: 'decimal',
          minimumFractionDigits: 2,
          maximumFractionDigits: 2,
        }).format(value);
      }

      const onClick = async (account: ISuggestedAccount, event: MouseEvent) => {
        if (event!=null && event.ctrlKey) {
          splittingAccounts.value.push({ suggestedAccount: account, amount: transaction.value?.amount || 0 });

          let sum = 0;
          splittingAccounts.value.forEach((account, index)=>{{
            account.amount = parseFloat((transaction.value?.amount /splittingAccounts.value.length).toFixed(2));
            sum += account.amount;
          }});
          const lastAccount = splittingAccounts.value[splittingAccounts.value.length - 1];
          lastAccount.amount += parseFloat((transaction.value.amount - sum).toFixed(2));
          return; // Beendet die Funktion, um den restlichen Code nicht auszuführen
        }

        try {
          const savedPeriodId: string | null = localStorage.getItem('selectedPeriod');
          if (savedPeriodId === null) {
            return;
          }
          await axios.patch('https://localhost:7123/transaction/journalEntry', {
            transactionId: transaction.value?.id,
            otherAccountId: account.accountId,
            accountingPeriodId: savedPeriodId,
          });
          window.location.reload(); // Seite neu laden
        } catch (error) {
          $q.notify({
            type: 'negative',
            message: 'There was an error updating transaction',
            caption: error
          });

        }
      };

      const splitAmount = async () => {
        try {

          const savedPeriodId: string | null = localStorage.getItem('selectedPeriod');
          if (savedPeriodId === null) {
            return;
          }
          const entries = splittingAccounts.value.map(x => ({
            amount: x.amount, otherAccountId: x.suggestedAccount.accountId
          }))
          await axios.patch('https://localhost:7123/transaction/journalEntry/split', {
            accountingPeriodId: savedPeriodId,
            transactionId: transaction.value?.id,
            entries: entries
          });
          window.location.reload(); // Seite neu laden
        } catch (error) {
          $q.notify({
            type: 'negative',
            message: 'Error updating the transaction: ' + error
          });;

        }
      }

      const handleEnter = () => {
        if (filteredAccounts.value.length === 1) {
          onClick(filteredAccounts.value[0], null);
        }
      };

      onMounted(async () => {
        await FetchTransaction();
        nextTick(() => {
          if (filterInput.value) {
            filterInput.value.focus();
          }
        });
      });


      return {
        FetchTransaction,
        transaction,
        formatDate,
        formatNumber,
        onClick,
        filterText,
        filteredAccounts,
        handleEnter,
        filterInput,
        splittingAccounts,
        splitAmount
      }

    }
  });
</script>

<style>
</style>
