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
  <div v-if="transaction" class="row q-pa-md">
    <div v-for="account in filteredAccounts" :key="account.id">
      <q-chip clickable @click="onClick(account)" size="md" :color="account.accountIconColor" :icon="account.accountIcon" text-color="white" square> {{account.accountName}}</q-chip>

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

  export default defineComponent({
    setup() {

      const transaction = ref<ITransaction | null>(null);
      const filterText = ref<string>("");
      const filterInput = ref(null);


      const filteredAccounts = computed(() => {
        if (!transaction.value) {
          return [];
        }
        return transaction.value.suggestedAccounts.filter((account) =>
          account.accountName.toLowerCase().includes(filterText.value.toLowerCase())
        );
      });

      const FetchTransaction = async () => {
        try {
          const response = await axios.get('https://localhost:7123/transaction/nextOpen');
          transaction.value = response.data;
        } catch (error) {
          console.error('There was an error fetching the transactions:', error);
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

      const onClick = async (account: ISuggestedAccount) => {
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
          console.error('Error updating the transaction:', error);
        }
      };

      const handleEnter = () => {
        if (filteredAccounts.value.length === 1) {
          onClick(filteredAccounts.value[0]);
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
        filterInput
      }

    }
  });
</script>

<style>
</style>
