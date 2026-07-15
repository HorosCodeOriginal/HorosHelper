---
name: frontend-builder
description: >-
  Baut moderne React/Next.js-Frontends. Verwenden bei Web-Apps, Stack-Wahl,
  Komponenten-Struktur oder UI/UX-Implementierung. React, Next.js, Tailwind,
  Komponenten-Patterns.
version: 1.0.0
---

# Frontend Builder

Baue wartbare, performante React- und Next.js-Frontends.

## Kernprinzipien

### 1. Komponenten-Komposition

UI in kleine, wiederverwendbare, einzweck-Komponenten zerlegen

### 2. State-Nähe

State so nah wie möglich dort halten, wo er genutzt wird

### 3. Performance by Default

Rendering, Code-Splitting und Asset-Loading optimieren

### 4. Developer Experience

Klare Namen, konsistente Patterns, hilfreiche Fehler

## Framework-Auswahl

### React (Vite) vs. Next.js

**Nutze React + Vite, wenn**:

- Nur Client-seitige Anwendung
- Keine SEO-Anforderungen
- Einfaches Deployment (statisches Hosting)
- Schnelleres initiales Setup

**Nutze Next.js, wenn**:

- SEO wichtig (Marketing-Sites, Blogs, E-Commerce)
- Server-Side Rendering nötig
- API-Routes erforderlich
- File-basiertes Routing bevorzugt
- Bildoptimierung kritisch

**Empfohlen für die meisten Projekte**: Next.js (App Router)

---

## Komponenten-Architektur

### Komponententypen

**1. Page Components** (Route-Einstiegspunkte):

```typescript
// app/users/page.tsx (Next.js App Router)
export default function UsersPage() {
  return (
    <div>
      <Header />
      <UserList />
      <Footer />
    </div>
  )
}
```

**2. Feature Components** (Business-Logik):

```typescript
// components/features/UserList.tsx
export function UserList() {
  const { data, isLoading } = useUsers()

  if (isLoading) return <LoadingSpinner />

  return (
    <div>
      {data.map(user => <UserCard key={user.id} user={user} />)}
    </div>
  )
}
```

**3. UI Components** (wiederverwendbar, keine Business-Logik):

```typescript
// components/ui/button.tsx
export function Button({ children, variant = 'primary', ...props }) {
  return (
    <button
      className={cn(buttonVariants[variant])}
      {...props}
    >
      {children}
    </button>
  )
}
```

### Komponenten-Best Practices

```typescript
// ✅ Good: Small, focused, typed
interface UserProfileProps {
  user: User
  onEdit?: () => void
}

export function UserProfile({ user, onEdit }: UserProfileProps) {
  return (
    <div className="flex gap-4">
      <Avatar src={user.avatar} alt={user.name} />
      <UserDetails user={user} />
      {onEdit && <Button onClick={onEdit}>Edit</Button>}
    </div>
  )
}

// ❌ Bad: Giant, untyped, unclear
export function UserProfile(props) {
  // 500 lines of JSX, multiple responsibilities
  return <div>...</div>
}
```

---

## State Management

### Entscheidungsbaum

```
How many components need this state?
│
├─ Eine Komponente → useState
├─ Parent + Children → Props oder useState + props
├─ Geschwister → Zum gemeinsamen Parent hochheben
├─ Weit verbreitet (Theme, Auth) → Context API
└─ Komplexer App-State → Zustand oder Redux
```

### Lokaler State (useState)

```typescript
// Für State auf Komponentenebene
function Counter() {
  const [count, setCount] = useState(0)
  const [isOpen, setIsOpen] = useState(false)

  return (
    <div>
      <button onClick={() => setCount(count + 1)}>{count}</button>
    </div>
  )
}
```

### Context API

```typescript
// Für app-weiten State (Theme, Auth, User)
const UserContext = createContext<UserContextType | undefined>(undefined)

export function UserProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<User | null>(null)

  return (
    <UserContext.Provider value={{ user, setUser }}>
      {children}
    </UserContext.Provider>
  )
}

export function useUser() {
  const context = useContext(UserContext)
  if (!context) throw new Error('useUser must be within UserProvider')
  return context
}
```

### Zustand (empfohlen für komplexen State)

```typescript
import { create } from 'zustand'

interface CounterStore {
  count: number
  increment: () => void
  decrement: () => void
  reset: () => void
}

export const useCounterStore = create<CounterStore>((set) => ({
  count: 0,
  increment: () => set((state) => ({ count: state.count + 1 })),
  decrement: () => set((state) => ({ count: state.count - 1 })),
  reset: () => set({ count: 0 })
}))

// Usage
function Counter() {
  const { count, increment } = useCounterStore()
  return <button onClick={increment}>{count}</button>
}
```

---

## Data Fetching

### React Query (empfohlen)

```typescript
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'

// Query (GET)
function Users() {
  const { data, isLoading, error } = useQuery({
    queryKey: ['users'],
    queryFn: fetchUsers,
    staleTime: 5 * 60 * 1000 // 5 minutes
  })

  if (isLoading) return <LoadingSpinner />
  if (error) return <ErrorMessage error={error} />

  return <UserList users={data} />
}

// Mutation (POST, PUT, DELETE)
function CreateUser() {
  const queryClient = useQueryClient()

  const mutation = useMutation({
    mutationFn: createUser,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['users'] })
    }
  })

  return (
    <button onClick={() => mutation.mutate({ name: 'John' })}>
      Create User
    </button>
  )
}
```

### Next.js Server Components (App Router)

```typescript
// app/users/page.tsx
// Server Component — fetcht auf dem Server
export default async function UsersPage() {
  const users = await fetchUsers() // Läuft auf dem Server

  return <UserList users={users} />
}

// Client Component — für Interaktivität
'use client'

export function UserList({ users }: { users: User[] }) {
  const [selected, setSelected] = useState<string | null>(null)

  return (
    <div>
      {users.map(user => (
        <UserCard
          key={user.id}
          user={user}
          onClick={() => setSelected(user.id)}
        />
      ))}
    </div>
  )
}
```

---

## Formular-Handling

### React Hook Form (empfohlen)

```typescript
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'

const loginSchema = z.object({
  email: z.string().email('Invalid email address'),
  password: z.string().min(8, 'Password must be at least 8 characters')
})

type LoginForm = z.infer<typeof loginSchema>

function LoginForm() {
  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting }
  } = useForm<LoginForm>({
    resolver: zodResolver(loginSchema)
  })

  const onSubmit = async (data: LoginForm) => {
    await login(data)
  }

  return (
    <form onSubmit={handleSubmit(onSubmit)}>
      <div>
        <input
          {...register('email')}
          type="email"
          placeholder="Email"
          className="border p-2"
        />
        {errors.email && (
          <span className="text-red-500">{errors.email.message}</span>
        )}
      </div>

      <div>
        <input
          {...register('password')}
          type="password"
          placeholder="Password"
          className="border p-2"
        />
        {errors.password && (
          <span className="text-red-500">{errors.password.message}</span>
        )}
      </div>

      <button type="submit" disabled={isSubmitting}>
        {isSubmitting ? 'Logging in...' : 'Login'}
      </button>
    </form>
  )
}
```

---

## Styling

### Tailwind CSS (empfohlen)

```typescript
// Installieren: @shadcn/ui als Komponentenbibliothek
function Button({ variant = 'primary', children, ...props }) {
  return (
    <button
      className={cn(
        'px-4 py-2 rounded font-medium transition-colors',
        {
          'bg-blue-500 text-white hover:bg-blue-600': variant === 'primary',
          'bg-gray-200 text-gray-900 hover:bg-gray-300': variant === 'secondary',
          'bg-red-500 text-white hover:bg-red-600': variant === 'danger'
        }
      )}
      {...props}
    >
      {children}
    </button>
  )
}
```

### CSS Modules (Alternative)

```typescript
// Button.module.css
.button {
  padding: 0.5rem 1rem;
  border-radius: 0.25rem;
}

.primary {
  background-color: blue;
  color: white;
}

// Button.tsx
import styles from './Button.module.css'

export function Button({ variant = 'primary', children }) {
  return (
    <button className={`${styles.button} ${styles[variant]}`}>
      {children}
    </button>
  )
}
```

---

## Performance-Optimierung

### React-Optimierung

```typescript
import { memo, useMemo, useCallback } from 'react'

// 1. Teure Berechnungen memoisieren
function DataTable({ data }) {
  const sortedData = useMemo(
    () => data.sort((a, b) => a.name.localeCompare(b.name)),
    [data]
  )

  return <Table data={sortedData} />
}

// 2. Callbacks memoisieren
function Parent() {
  const handleClick = useCallback(() => {
    console.log('Clicked')
  }, [])

  return <ExpensiveChild onClick={handleClick} />
}

// 3. Komponenten memoisieren
const ExpensiveChild = memo(function ExpensiveChild({ onClick }) {
  return <button onClick={onClick}>Click</button>
})
```

### Next.js-Optimierung

```typescript
// 1. Bildoptimierung
import Image from 'next/image'

<Image
  src="/photo.jpg"
  alt="Photo"
  width={500}
  height={300}
  priority // Above the fold
/>

// 2. Schriftoptimierung
import { Inter } from 'next/font/google'

const inter = Inter({ subsets: ['latin'] })

export default function RootLayout({ children }) {
  return (
    <html className={inter.className}>
      <body>{children}</body>
    </html>
  )
}

// 3. Dynamische Imports (Code-Splitting)
import dynamic from 'next/dynamic'

const HeavyComponent = dynamic(() => import('./HeavyComponent'), {
  loading: () => <LoadingSpinner />
})
```

---

## Fehlerbehandlung

### Error Boundary

```typescript
'use client'

import { Component, ReactNode } from 'react'

interface Props {
  children: ReactNode
  fallback?: ReactNode
}

interface State {
  hasError: boolean
  error?: Error
}

export class ErrorBoundary extends Component<Props, State> {
  constructor(props: Props) {
    super(props)
    this.state = { hasError: false }
  }

  static getDerivedStateFromError(error: Error): State {
    return { hasError: true, error }
  }

  render() {
    if (this.state.hasError) {
      return this.props.fallback || (
        <div className="p-4 bg-red-50 border border-red-200">
          <h2 className="text-red-800">Something went wrong</h2>
          <p className="text-red-600">{this.state.error?.message}</p>
        </div>
      )
    }

    return this.props.children
  }
}
```

### Next.js-Fehlerbehandlung

```typescript
// app/error.tsx
'use client'

export default function Error({
  error,
  reset
}: {
  error: Error
  reset: () => void
}) {
  return (
    <div>
      <h2>Something went wrong!</h2>
      <button onClick={reset}>Try again</button>
    </div>
  )
}
```

---

## Ordnerstruktur

### Next.js App Router

```
app/
├── (auth)/              # Route group (auth pages)
│   ├── login/
│   └── signup/
├── (dashboard)/         # Route group (dashboard)
│   ├── layout.tsx
│   ├── page.tsx
│   └── settings/
├── api/                 # API routes
│   └── users/
│       └── route.ts
└── layout.tsx           # Root layout

components/
├── ui/                  # shadcn/ui components
│   ├── button.tsx
│   ├── input.tsx
│   └── dialog.tsx
├── features/            # Feature components
│   ├── UserList.tsx
│   └── UserProfile.tsx
└── layouts/             # Layout components
    ├── Header.tsx
    └── Footer.tsx

lib/
├── utils.ts             # Utility functions
├── api.ts               # API client
└── validation.ts        # Zod schemas

hooks/
├── useUser.ts
└── useDebounce.ts

stores/
└── userStore.ts         # Zustand stores
```

---

## TypeScript-Best Practices

```typescript
// 1. Komponenten-Props typisieren
interface ButtonProps {
  variant?: 'primary' | 'secondary' | 'danger'
  children: ReactNode
  onClick?: () => void
}

export function Button({ variant = 'primary', children, onClick }: ButtonProps) {
  return <button onClick={onClick}>{children}</button>
}

// 2. API-Responses typisieren
interface User {
  id: string
  name: string
  email: string
}

async function fetchUsers(): Promise<User[]> {
  const res = await fetch('/api/users')
  return res.json()
}

// 3. State typisieren
const [user, setUser] = useState<User | null>(null)
const [isLoading, setIsLoading] = useState<boolean>(false)
```

---

## Zusammenfassung

Gute Frontends:

- ✅ Nutzen Next.js für die meisten Projekte (SEO, Performance, DX)
- ✅ Zerlegen UI in kleine, typisierte Komponenten
- ✅ Wählen passendes State Management (useState → Context → Zustand)
- ✅ Nutzen React Query für Server-State
- ✅ Stylen mit Tailwind CSS + shadcn/ui
- ✅ Optimieren mit Memoization und Code-Splitting
- ✅ Behandeln Fehler sauber mit Error Boundaries
- ✅ Folgen konsistenter Ordnerstruktur

---

## Verwandte Ressourcen

**Verwandte Skills**:

- `api-designer` — für Backend-APIs zum Konsumieren
- `ux-designer` — für UX-Designs zur Umsetzung
- `deployment-advisor` — für Hosting von Next.js/React-Apps

**Verwandte Patterns**:

- `META/DECISION-FRAMEWORK.md` — Frontend-Framework-Auswahl
- `STANDARDS/architecture-patterns/component-patterns.md` — Komponenten-Design-Patterns (wenn erstellt)

**Verwandte Playbooks**:

- `PLAYBOOKS/setup-nextjs-project.md` — Next.js-Projekt-Setup (wenn erstellt)
- `PLAYBOOKS/optimize-frontend-performance.md` — Performance-Optimierung (wenn erstellt)
