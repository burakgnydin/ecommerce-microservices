import {
  type ChangeEvent,
  type FormEvent,
  forwardRef,
  memo,
  type ReactNode,
  useEffect,
  useRef,
  useState,
} from 'react'
import { motion, useAnimation, useInView, useMotionTemplate, useMotionValue } from 'framer-motion'
import { Eye, EyeOff } from 'lucide-react'
import { cn } from '@/lib/utils'

// ==================== Input Component ====================

const Input = memo(
  forwardRef(function Input(
    { className, type, ...props }: React.InputHTMLAttributes<HTMLInputElement>,
    ref: React.ForwardedRef<HTMLInputElement>,
  ) {
    const radius = 100
    const [visible, setVisible] = useState(false)

    const mouseX = useMotionValue(0)
    const mouseY = useMotionValue(0)

    function handleMouseMove({ currentTarget, clientX, clientY }: React.MouseEvent<HTMLDivElement>) {
      const { left, top } = currentTarget.getBoundingClientRect()
      mouseX.set(clientX - left)
      mouseY.set(clientY - top)
    }

    return (
      <motion.div
        style={{
          background: useMotionTemplate`
        radial-gradient(
          ${visible ? radius + 'px' : '0px'} circle at ${mouseX}px ${mouseY}px,
          #3b82f6,
          transparent 80%
        )
      `,
        }}
        onMouseMove={handleMouseMove}
        onMouseEnter={() => setVisible(true)}
        onMouseLeave={() => setVisible(false)}
        className="group/input rounded-lg p-[2px] transition duration-300"
      >
        <input
          type={type}
          className={cn(
            'flex h-10 w-full rounded-md border-none bg-secondary px-3 py-2 text-sm text-foreground shadow-[0px_2px_3px_-1px_rgba(0,0,0,0.1),0px_1px_0px_0px_rgba(25,28,33,0.02),0px_0px_0px_1px_rgba(25,28,33,0.08)] transition duration-400 file:border-0 file:bg-transparent file:text-sm file:font-medium placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-[2px] focus-visible:ring-primary/40 disabled:cursor-not-allowed disabled:opacity-50 group-hover/input:shadow-none',
            className,
          )}
          ref={ref}
          {...props}
        />
      </motion.div>
    )
  }),
)
Input.displayName = 'Input'

// ==================== BoxReveal Component ====================

type BoxRevealProps = {
  children: ReactNode
  width?: string
  boxColor?: string
  duration?: number
  overflow?: string
  position?: string
  className?: string
}

const BoxReveal = memo(function BoxReveal({
  children,
  width = 'fit-content',
  boxColor,
  duration,
  overflow = 'hidden',
  position = 'relative',
  className,
}: BoxRevealProps) {
  const mainControls = useAnimation()
  const slideControls = useAnimation()
  const ref = useRef(null)
  const isInView = useInView(ref, { once: true })

  useEffect(() => {
    if (isInView) {
      slideControls.start('visible')
      mainControls.start('visible')
    } else {
      slideControls.start('hidden')
      mainControls.start('hidden')
    }
  }, [isInView, mainControls, slideControls])

  return (
    <section
      ref={ref}
      style={{
        position: position as 'relative' | 'absolute' | 'fixed' | 'sticky' | 'static',
        width,
        overflow,
      }}
      className={className}
    >
      <motion.div
        variants={{ hidden: { opacity: 0, y: 75 }, visible: { opacity: 1, y: 0 } }}
        initial="hidden"
        animate={mainControls}
        transition={{ duration: duration ?? 0.5, delay: 0.25 }}
      >
        {children}
      </motion.div>
      <motion.div
        variants={{ hidden: { left: 0 }, visible: { left: '100%' } }}
        initial="hidden"
        animate={slideControls}
        transition={{ duration: duration ?? 0.5, ease: 'easeIn' }}
        style={{
          position: 'absolute',
          top: 4,
          bottom: 4,
          left: 0,
          right: 0,
          zIndex: 20,
          background: boxColor ?? 'var(--color-primary)',
          borderRadius: 4,
        }}
      />
    </section>
  )
})

// ==================== Ripple Component ====================

type RippleProps = {
  mainCircleSize?: number
  mainCircleOpacity?: number
  numCircles?: number
  className?: string
}

const Ripple = memo(function Ripple({
  mainCircleSize = 210,
  mainCircleOpacity = 0.24,
  numCircles = 11,
  className = '',
}: RippleProps) {
  return (
    <section
      className={cn(
        'absolute inset-0 flex max-w-[50%] items-center justify-center bg-secondary [mask-image:linear-gradient(to_bottom,black,transparent)]',
        className,
      )}
    >
      {Array.from({ length: numCircles }, (_, i) => {
        const size = mainCircleSize + i * 70
        const opacity = mainCircleOpacity - i * 0.03
        const animationDelay = `${i * 0.06}s`
        const borderStyle = i === numCircles - 1 ? 'dashed' : 'solid'

        return (
          <span
            key={size}
            className="absolute animate-ripple rounded-full border border-foreground/10 bg-foreground/10"
            style={{
              width: `${size}px`,
              height: `${size}px`,
              opacity,
              animationDelay,
              borderStyle,
              top: '50%',
              left: '50%',
              transform: 'translate(-50%, -50%)',
            }}
          />
        )
      })}
    </section>
  )
})

// ==================== OrbitingCircles Component ====================

type OrbitingCirclesProps = {
  className?: string
  children: ReactNode
  reverse?: boolean
  duration?: number
  delay?: number
  radius?: number
  path?: boolean
}

const OrbitingCircles = memo(function OrbitingCircles({
  className,
  children,
  reverse = false,
  duration = 20,
  delay = 10,
  radius = 50,
  path = true,
}: OrbitingCirclesProps) {
  return (
    <>
      {path && (
        <svg xmlns="http://www.w3.org/2000/svg" className="pointer-events-none absolute inset-0 size-full">
          <circle className="stroke-foreground/10" cx="50%" cy="50%" r={radius} fill="none" />
        </svg>
      )}
      <section
        style={{ '--duration': duration, '--radius': radius, '--delay': -delay } as React.CSSProperties}
        className={cn(
          'absolute flex size-full transform-gpu animate-orbit items-center justify-center rounded-full border border-border bg-card [animation-delay:calc(var(--delay)*1000ms)]',
          { '[animation-direction:reverse]': reverse },
          className,
        )}
      >
        {children}
      </section>
    </>
  )
})

// ==================== TechOrbitDisplay Component ====================

type IconConfig = {
  className?: string
  duration?: number
  delay?: number
  radius?: number
  path?: boolean
  reverse?: boolean
  component: () => ReactNode
}

type TechOrbitDisplayProps = {
  iconsArray: IconConfig[]
  text?: string
}

const TechOrbitDisplay = memo(function TechOrbitDisplay({ iconsArray, text = 'E-Ticaret' }: TechOrbitDisplayProps) {
  return (
    <section className="relative flex h-full w-full flex-col items-center justify-center overflow-hidden rounded-lg">
      <motion.span
        className="pointer-events-none whitespace-pre-wrap text-center font-serif text-6xl font-semibold leading-none"
        animate={{ color: ['#3b82f6', '#ffffff', '#3b82f6'] }}
        transition={{ duration: 5, repeat: Infinity, ease: 'easeInOut' }}
      >
        {text}
      </motion.span>

      {iconsArray.map((icon, index) => (
        <OrbitingCircles
          key={index}
          className={icon.className}
          duration={icon.duration}
          delay={icon.delay}
          radius={icon.radius}
          path={icon.path}
          reverse={icon.reverse}
        >
          {icon.component()}
        </OrbitingCircles>
      ))}
    </section>
  )
})

// ==================== AnimatedForm Component ====================

type FieldType = 'text' | 'email' | 'password'

type Field = {
  label: string
  required?: boolean
  type: FieldType
  placeholder?: string
  onChange: (event: ChangeEvent<HTMLInputElement>) => void
}

type AnimatedFormProps = {
  header: string
  subHeader?: string
  fields: Field[]
  submitButton: string
  textVariantButton?: string
  errorField?: string
  isSubmitting?: boolean
  onSubmit: (event: FormEvent<HTMLFormElement>) => void
  goTo?: (event: React.MouseEvent<HTMLButtonElement>) => void
}

type Errors = Record<string, string>

const AnimatedForm = memo(function AnimatedForm({
  header,
  subHeader,
  fields,
  submitButton,
  textVariantButton,
  errorField,
  isSubmitting,
  onSubmit,
  goTo,
}: AnimatedFormProps) {
  const [visible, setVisible] = useState(false)
  const [errors, setErrors] = useState<Errors>({})

  const toggleVisibility = () => setVisible(!visible)

  const validateForm = (event: FormEvent<HTMLFormElement>) => {
    const currentErrors: Errors = {}
    fields.forEach((field) => {
      const value = (event.target as HTMLFormElement)[field.label]?.value

      if (field.required && !value) {
        currentErrors[field.label] = `${field.label} zorunludur`
      }
      if (field.type === 'email' && value && !/\S+@\S+\.\S+/.test(value)) {
        currentErrors[field.label] = 'Geçersiz e-posta adresi'
      }
      if (field.type === 'password' && value && value.length < 6) {
        currentErrors[field.label] = 'Şifre en az 6 karakter olmalı'
      }
    })
    return currentErrors
  }

  const handleSubmit = (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    const formErrors = validateForm(event)

    if (Object.keys(formErrors).length === 0) {
      onSubmit(event)
    } else {
      setErrors(formErrors)
    }
  }

  return (
    <section className="mx-auto flex w-96 flex-col gap-4 max-md:w-full">
      <BoxReveal duration={0.3}>
        <h2 className="text-3xl font-bold text-foreground">{header}</h2>
      </BoxReveal>

      {subHeader && (
        <BoxReveal duration={0.3} className="pb-2">
          <p className="max-w-sm text-sm text-muted-foreground">{subHeader}</p>
        </BoxReveal>
      )}

      <form onSubmit={handleSubmit}>
        <section className="mb-4 grid grid-cols-1">
          {fields.map((field) => (
            <section key={field.label} className="flex flex-col gap-2">
              <BoxReveal duration={0.3}>
                <label htmlFor={field.label} className="text-sm font-medium text-foreground">
                  {field.label} <span className="text-destructive">*</span>
                </label>
              </BoxReveal>

              <BoxReveal width="100%" duration={0.3} className="flex w-full flex-col space-y-2">
                <section className="relative">
                  <Input
                    type={field.type === 'password' ? (visible ? 'text' : 'password') : field.type}
                    id={field.label}
                    name={field.label}
                    placeholder={field.placeholder}
                    onChange={field.onChange}
                  />

                  {field.type === 'password' && (
                    <button
                      type="button"
                      onClick={toggleVisibility}
                      className="absolute inset-y-0 right-0 flex items-center pr-3 text-sm leading-5 text-muted-foreground"
                    >
                      {visible ? <Eye className="h-5 w-5" /> : <EyeOff className="h-5 w-5" />}
                    </button>
                  )}
                </section>

                <section className="h-4">
                  {errors[field.label] && <p className="text-xs text-destructive">{errors[field.label]}</p>}
                </section>
              </BoxReveal>
            </section>
          ))}
        </section>

        <BoxReveal width="100%" duration={0.3}>
          {errorField && <p className="mb-4 text-sm text-destructive">{errorField}</p>}
        </BoxReveal>

        <BoxReveal width="100%" duration={0.3} overflow="visible">
          <button
            className="group/btn relative block h-10 w-full rounded-md bg-primary font-medium text-primary-foreground shadow-[0px_1px_0px_0px_#ffffff40_inset,0px_-1px_0px_0px_#ffffff40_inset] outline-hidden hover:cursor-pointer hover:bg-primary-hover disabled:cursor-not-allowed disabled:opacity-60"
            type="submit"
            disabled={isSubmitting}
          >
            {submitButton} &rarr;
            <BottomGradient />
          </button>
        </BoxReveal>

        {textVariantButton && goTo && (
          <BoxReveal duration={0.3}>
            <section className="mt-4 text-center hover:cursor-pointer">
              <button type="button" className="text-sm text-primary outline-hidden hover:cursor-pointer" onClick={goTo}>
                {textVariantButton}
              </button>
            </section>
          </BoxReveal>
        )}
      </form>
    </section>
  )
})

const BottomGradient = () => {
  return (
    <>
      <span className="absolute -bottom-px inset-x-0 block h-px w-full bg-gradient-to-r from-transparent via-cyan-500 to-transparent opacity-0 transition duration-500 group-hover/btn:opacity-100" />
      <span className="absolute -bottom-px inset-x-10 mx-auto block h-px w-1/2 bg-gradient-to-r from-transparent via-indigo-500 to-transparent opacity-0 blur-sm transition duration-500 group-hover/btn:opacity-100" />
    </>
  )
}

export { Input, BoxReveal, Ripple, OrbitingCircles, TechOrbitDisplay, AnimatedForm, BottomGradient }
